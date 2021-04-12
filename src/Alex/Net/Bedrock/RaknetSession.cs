#region LICENSE

// The contents of this file are subject to the Common Public Attribution
// License Version 1.0. (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// https://github.com/NiclasOlofsson/MiNET/blob/master/LICENSE.
// The License is based on the Mozilla Public License Version 1.1, but Sections 14
// and 15 have been added to cover use of software over a computer network and
// provide for limited attribution for the Original Developer. In addition, Exhibit A has
// been modified to be consistent with Exhibit B.
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// The Original Code is MiNET.
// 
// The Original Developer is the Initial Developer.  The Initial Developer of
// the Original Code is Niclas Olofsson.
// 
// All portions of the code written by Niclas Olofsson are Copyright (c) 2014-2020 Niclas Olofsson.
// All Rights Reserved.

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.Net.Bedrock.Raknet;
using Alex.Utils;
using log4net;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using MiNET.Utils.Collections;
using MiNET.Utils.Nbt;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using ConnectionInfo = Alex.API.Utils.ConnectionInfo;
using Datagram = Alex.Net.Bedrock.Raknet.Datagram;
using LogManager = NLog.LogManager;

namespace Alex.Net.Bedrock
{
	public class RaknetSession : INetworkHandler
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		private readonly RaknetConnection _packetSender;

		private long _lastOrderingIndex   = -1; // That's the first message with wrapper


		private readonly ConcurrentPriorityQueue<int, Packet> _orderingBufferQueue = new ConcurrentPriorityQueue<int, Packet>();
		private          CancellationTokenSource                          _cancellationToken;
		private          Thread                                           _orderedQueueProcessingThread;

		public ConnectionInfo ConnectionInfo { get; }

		public ICustomMessageHandler CustomMessageHandler { get; set; }
		
		public IPEndPoint EndPoint { get; }
		public short MtuSize { get; }
		public long NetworkIdentifier { get; set; }

		//public int DatagramSequenceNumber = -1;
		public int ReliableMessageNumber = -1;
		public int SplitPartId = 0;
		public int OrderingIndex = -1;
	
		public bool Evicted { get; set; }

		public ConnectionState State { get; set; } = ConnectionState.Unconnected;

		public DateTime LastUpdatedTime { get; set; }
		//public int ResendCount { get; set; }

		public SlidingWindow SlidingWindow { get; }

		public const long InactivityTimeout = 30000;

		public ConcurrentDictionary<int, SplitPartPacket[]> Splits { get; } = new ConcurrentDictionary<int, SplitPartPacket[]>();
		private ConcurrentQueue<int> OutgoingAckQueue { get; } = new ConcurrentQueue<int>();
		private ConcurrentQueue<int> OutgoingNackQueue { get; } = new ConcurrentQueue<int>();
		public ConcurrentDictionary<int, Datagram> WaitingForAckQueue { get; } = new ConcurrentDictionary<int, Datagram>();

		public short CompressionThreshold { get; set; } = -1;
		private Timer _tickerHighPrecisionTimer;
		public RaknetSession(ConnectionInfo connectionInfo, RaknetConnection packetSender, IPEndPoint endPoint, short mtuSize, ICustomMessageHandler messageHandler = null)
		{
			_packetSender = packetSender;
			ConnectionInfo = connectionInfo;
			CustomMessageHandler = messageHandler ?? new DefaultMessageHandler();
			EndPoint = endPoint;
			MtuSize = mtuSize;

			_cancellationToken = new CancellationTokenSource();

			SlidingWindow = new SlidingWindow(mtuSize);
			_tickerHighPrecisionTimer = new Timer(SendTick, null, 10, 10);
		}

		/// <summary>
		///     Main receive entry to this layer. Will receive and handle messages
		///     on RakNet message level. May come from either UDP or TCP, matters not.
		/// </summary>
		/// <param name="message"></param>
		internal void HandleRakMessage(Packet message)
		{
			if (message == null) return;

			// This is not completely finished. Ordering and sequence streams (32 unique channels/streams each)
			// needs to work by their channel index. Right now, it's only one channel per reliability type.
			// According to Dylan order and sequence streams can run on the same channel, but documentation
			// says it can not. So I'll go with documentation until proven wrong.

			switch (message.ReliabilityHeader.Reliability)
			{
				case Reliability.ReliableOrdered:
				case Reliability.ReliableOrderedWithAckReceipt:
					AddToOrderedChannel(message);
					break;
				case Reliability.UnreliableSequenced:
				case Reliability.ReliableSequenced:
					Log.Warn($"Sequenced: {message.ReliabilityHeader.SequencingIndex} | OrderIndex: {message.ReliabilityHeader.OrderingIndex}");
					AddToOrderedChannel(message);
					break;
				case Reliability.Unreliable:
				case Reliability.UnreliableWithAckReceipt:
				case Reliability.Reliable:
				case Reliability.ReliableWithAckReceipt:
					HandlePacket(message);
					break;
				case Reliability.Undefined:
					Log.Error($"Receive packet with undefined reliability");
					break;
				default:
					Log.Warn($"Receive packet with unexpected reliability={message.ReliabilityHeader.Reliability}");
					break;
			}
		}

		private AutoResetEvent _orderingResetEvent = new AutoResetEvent(false);
		private void AddToOrderedChannel(Packet message)
		{
			try
			{
				if (_cancellationToken.Token.IsCancellationRequested) return;
				
				var current  = message.ReliabilityHeader.OrderingIndex.IntValue();

				var last = Interlocked.CompareExchange(ref _lastOrderingIndex, current, current - 1);
				if (last == current - 1)
				{
					HandlePacket(message);
					
					if (_orderingBufferQueue.Count > 0)
						StartOrderingThread();
				}
				else
				{
					if (current <= last)
						return;
					
					_orderingBufferQueue.Enqueue(current, message);
					
					if (!IsOutOfOrder)
					{
						IsOutOfOrder = true;
						Log.Warn($"Network channel out of order. Expected {last + 1}, but was {current}.");
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Something went wrong!");
			}
		}

		private void StartOrderingThread()
		{
			_orderingResetEvent.Set();
			if (_orderedQueueProcessingThread != null)
				return;
			
			_orderedQueueProcessingThread = new Thread(ProcessOrderedQueue)
			{
				IsBackground = true,
				Name = $"Ordering Thread [{EndPoint}]"
			};
			_orderedQueueProcessingThread.Start();
			if (Log.IsDebugEnabled) Log.Warn($"Started network ordering thread.");
		}

		public bool IsOutOfOrder
		{
			get => _isOutOfOrder;
			private set
			{
				_isOutOfOrder = value;
			}
		}

		private void ProcessOrderedQueue()
		{
			Log.Info($"Ordering start");
			try
			{
				while (!_cancellationToken.IsCancellationRequested)
				{

					while (_orderingBufferQueue.TryDequeue(out KeyValuePair<int, Packet> pair)
					       && !_cancellationToken.IsCancellationRequested)
					{
						var last = Interlocked.CompareExchange(ref _lastOrderingIndex, pair.Key, pair.Key - 1);
						if (last == pair.Key - 1)
						{
							//_orderingBufferQueue.TryDequeue(out _);
							
							HandlePacket(pair.Value);
						}
						else if (pair.Key < last)
						{
							//_orderingBufferQueue.TryDequeue(out _);
							pair.Value.PutPool();
						}
						else if (pair.Key > last)
						{
							_orderingBufferQueue.Enqueue(pair.Key, pair.Value);
						}
					}

					if (!_orderingResetEvent.WaitOne(500)) //Keep the thread alive for longer.
						break;
				}
			}
			catch (ObjectDisposedException)
			{
				// Ignore. Comes from the reset events being waited on while being disposed. Not a problem.
			}
			catch (Exception e)
			{
				Log.Error(e, $"Exit receive handler task for player");
			}
			finally
			{
				_orderingResetEvent?.Reset();
				_orderedQueueProcessingThread = null;
				
				if (_orderingBufferQueue.IsEmpty)
					IsOutOfOrder = false;
				
				Log.Info($"Ordering stop");
			}
		}

		private void HandlePacket(Packet message)
		{

			if (message == null) return;

			/*if ((message.ReliabilityHeader.Reliability == Reliability.ReliableSequenced 
			     || message.ReliabilityHeader.Reliability == Reliability.UnreliableSequenced)
			    && message.ReliabilityHeader.SequencingIndex < Interlocked.Read(ref _lastSequencingIndex))
			{
				return;
			}
			*/
			try
			{
				//	RakOfflineHandler.TraceReceive(Log, message);
				TraceReceive(message);

				if (message.Id < (int) DefaultMessageIdTypes.ID_USER_PACKET_ENUM)
				{
					// Standard RakNet online message handlers
					switch (message)
					{
						case ConnectedPing connectedPing:
							HandleConnectedPing(connectedPing);

							break;

						case ConnectedPong connectedPong:
							//	HandleConnectedPong(connectedPong);

							break;

						case DetectLostConnections _:
							break;

						case ConnectionRequest connectionRequest:
							HandleConnectionRequest(connectionRequest);

							break;

						case ConnectionRequestAccepted connectionRequestAccepted:
							HandleConnectionRequestAccepted(connectionRequestAccepted);

							break;

						case NewIncomingConnection newIncomingConnection:
							HandleNewIncomingConnection(newIncomingConnection);

							break;

						case DisconnectionNotification _:
							HandleDisconnectionNotification();

							break;

						default:
							Log.Error(
								$"Unhandled packet: {message.GetType().Name} 0x{message.Id:X2} IP {EndPoint.Address}");

							if (Log.IsDebugEnabled)
								Log.Warn($"Unknown packet 0x{message.Id:X2}\n{Packet.HexDump(message.Bytes)}");

							break;
					}
				}
				else
				{
					try
					{
						CustomMessageHandler.HandlePacket(message);
					}
					catch (Exception e)
					{
						// ignore
						Log.Warn(e, $"Custom message handler error");
					}
				}

				if (message.Timer.IsRunning)
				{
					long elapsedMilliseconds = message.Timer.ElapsedMilliseconds;

					if (elapsedMilliseconds > 1000)
					{
						Log.Warn(
							$"Packet (0x{message.Id:x2}) handling too long {elapsedMilliseconds}ms ({message.ToString()})");
					}
				}
				else
				{
					Log.Warn("Packet (0x{0:x2}) timer not started.", message.Id);
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Packet handling");

				throw;
			}
			finally
			{
				message?.PutPool();
			}
		}

		protected virtual void HandleConnectedPing(ConnectedPing message)
		{
			var packet = ConnectedPong.CreateObject();
			packet.sendpingtime = message.sendpingtime;
			packet.sendpongtime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			SendPacket(packet);
		}

		protected virtual void HandleConnectionRequest(ConnectionRequest message)
		{
			Log.Debug("Connection request from: {0}", EndPoint.Address);

			var response = ConnectionRequestAccepted.CreateObject();
			response.NoBatch = true;
			response.systemAddress = new IPEndPoint(IPAddress.Loopback, 19132);
			response.systemAddresses = new IPEndPoint[20];
			response.systemAddresses[0] = new IPEndPoint(IPAddress.Loopback, 19132);
			response.incomingTimestamp = message.timestamp;
			response.serverTimestamp = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

			for (int i = 1; i < 20; i++)
			{
				response.systemAddresses[i] = new IPEndPoint(IPAddress.Any, 19132);
			}

			SendPacket(response);
		}

		protected virtual void HandleNewIncomingConnection(NewIncomingConnection message)
		{
			Log.Debug($"New incoming connection from {EndPoint.Address} {EndPoint.Port}");

			State = ConnectionState.Connected;
		}

		private void HandleConnectionRequestAccepted(ConnectionRequestAccepted message)
		{
			SendNewIncomingConnection();

			State = ConnectionState.Connected;

			CustomMessageHandler?.Connected();
		}

		public void SendNewIncomingConnection()
		{
			var packet = NewIncomingConnection.CreateObject();
			packet.clientendpoint = EndPoint;
			packet.systemAddresses = new IPEndPoint[20];
			for (int i = 0; i < 20; i++)
			{
				packet.systemAddresses[i] = new IPEndPoint(IPAddress.Any, 0);
			}

			SendPacket(packet);
		}


		protected virtual void HandleDisconnectionNotification()
		{
			Disconnect("Server requested disconnect", false);
		}

		public virtual void Disconnect(string reason, bool sendDisconnect = true)
		{
			CustomMessageHandler?.Disconnect(reason, sendDisconnect);
			Close();
		}
		
		// MCPE Login handling
		
		private ConcurrentQueue<Packet> _sendQueue = new ConcurrentQueue<Packet>();
		public void SendPacket(Packet packet)
		{
			if (packet == null) return;

			if (State == ConnectionState.Unconnected)
			{
				if (Log.IsDebugEnabled) Log.Debug($"Ignoring send of packet {packet.GetType().Name} because session is not connected");
				packet.PutPool();
				return;
			}

			TraceSend(packet);

			_sendQueue.Enqueue(packet);
		}

		internal static void TraceReceive(Packet message)
		{
			if (!Log.IsTraceEnabled)
				return;

			try
			{
				string name      = message.GetType().Name;
				string property1 = Config.GetProperty("TracePackets.Include", ".*");
				string property2 = Config.GetProperty("TracePackets.Exclude", (string) null);
				int    property3 = Config.GetProperty("TracePackets.Verbosity", 0);
				int    property4 = Config.GetProperty("TracePackets.Verbosity." + name, property3);

				if (!Regex.IsMatch(name, property1)
				    || !string.IsNullOrWhiteSpace(property2) && Regex.IsMatch(name, property2))
					return;

				if (property4 == 0)
					Log.Trace(
						string.Format(
							"> Receive: {0} (0x{1:x2}): {2}", (object) message.Id, (object) message.Id,
							(object) message.GetType().Name));
				else if (property4 == 1 || property4 == 3)
				{
					JsonSerializerSettings settings = new JsonSerializerSettings()
					{
						PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
						TypeNameHandling = TypeNameHandling.Auto,
						Formatting = Formatting.Indented,
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

					settings.Converters.Add((JsonConverter) new StringEnumConverter());
					settings.Converters.Add((JsonConverter) new NbtIntConverter());
					settings.Converters.Add((JsonConverter) new NbtStringConverter());
					settings.Converters.Add((JsonConverter) new IPAddressConverter());
					settings.Converters.Add((JsonConverter) new IPEndPointConverter());
					string str = JsonConvert.SerializeObject((object) message, settings);

					Log.Trace(
						string.Format(
							"> Receive: {0} (0x{1:x2}): {2}\n{3}", (object) message.Id, (object) message.Id,
							(object) message.GetType().Name, (object) str));
				}
				else
				{
					if (property4 != 2)
						return;

					Log.Trace(
						string.Format(
							"> Receive: {0} (0x{1:x2}): {2}\n{3}", (object) message.Id, (object) message.Id,
							(object) message.GetType().Name, (object) Packet.HexDump(message.Bytes)));
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error when printing trace");
			}
		}

		internal static void TraceSend(Packet message)
		{
			if (!Log.IsTraceEnabled)
				return;

			try
			{
				string name      = message.GetType().Name;
				string property1 = Config.GetProperty("TracePackets.Include", ".*");
				string property2 = Config.GetProperty("TracePackets.Exclude", (string) null);
				int    property3 = Config.GetProperty("TracePackets.Verbosity", 0);
				int    property4 = Config.GetProperty("TracePackets.Verbosity." + name, property3);

				if (!Regex.IsMatch(name, property1)
				    || !string.IsNullOrWhiteSpace(property2) && Regex.IsMatch(name, property2))
					return;

				if (property4 == 0)
					Log.Trace(
						string.Format(
							"<    Send: {0} (0x{1:x2}): {2}", (object) message.Id, (object) message.Id,
							(object) message.GetType().Name));
				else if (property4 == 1 || property4 == 3)
				{
					JsonSerializerSettings settings = new JsonSerializerSettings()
					{
						PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
						TypeNameHandling = TypeNameHandling.Auto,
						Formatting = Formatting.Indented,
						DefaultValueHandling = DefaultValueHandling.Include,
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

					settings.Converters.Add((JsonConverter) new StringEnumConverter());
					settings.Converters.Add((JsonConverter) new NbtIntConverter());
					settings.Converters.Add((JsonConverter) new NbtStringConverter());
					settings.Converters.Add((JsonConverter) new IPAddressConverter());
					settings.Converters.Add((JsonConverter) new IPEndPointConverter());
					string str = JsonConvert.SerializeObject((object) message, settings);

					Log.Trace(
						string.Format(
							"<    Send: {0} (0x{1:x2}): {2}\n{3}", (object) message.Id, (object) message.Id,
							(object) message.GetType().Name, (object) str));
				}
				else
				{
					if (property4 != 2 && property4 != 3)
						return;

					Log.Trace(
						string.Format(
							"<    Send: {0} (0x{1:x2}): {2}\n{3}", (object) message.Id, (object) message.Id,
							(object) message.GetType().Name, (object) Packet.HexDump(message.Bytes)));
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error when printing trace");
			}
		}


		private int _tickCounter = 0;
		private object _updateSync = new object();
		private void SendTick(object obj)
		{
			if (!Monitor.TryEnter(_updateSync, 0))
				return;

			try
			{
				SendAckQueue();
				SendNackQueue();

				if (_tickCounter++ >= 5)
				{
					Update();
					_tickCounter = 0;
				}

				SendQueue();
				UpdateTick();
			}
			catch (Exception e)
			{
				Log.Warn(e);
			}
			finally
			{
				Monitor.Exit(_updateSync);
			}
		}

		private void UpdateTick()
		{
			if (Evicted) return;
			
			try
			{
				//UpdateSync.Wait();
			
				if (WaitingForAckQueue.Count == 0) return;

				//long rto = session.SlidingWindow.GetRtoForRetransmission();
				//if (rto == 0) return;

				int transmissionBandwidth = SlidingWindow.GetRetransmissionBandwidth(UnackedBytes);
				
				var queue = WaitingForAckQueue;

				foreach (KeyValuePair<int, Datagram> datagramPair in queue)
				{
					if (Evicted) return;

					Datagram datagram = datagramPair.Value;
					
					if (!datagram.Timer.IsRunning)
					{
						Log.Error($"Timer not running for #{datagram.Header.DatagramSequenceNumber}");
						datagram.Timer.Restart();
						continue;
					}

					//if (session.Rtt == -1) return;

					long elapsedTime = datagram.Timer.ElapsedMilliseconds;
					long datagramTimeout = datagram.RetransmissionTimeOut;
					datagramTimeout = Math.Min(datagramTimeout, 3000);
					datagramTimeout = Math.Max(datagramTimeout, 100);

					if (datagram.RetransmitImmediate || elapsedTime >= datagramTimeout)
					{
						if (transmissionBandwidth < datagram.Bytes.Length)
							break;

						if (!Evicted && WaitingForAckQueue.TryRemove(datagramPair.Key, out datagram))
						{
							transmissionBandwidth -= datagram.Bytes.Length;
							UnackedBytes -= datagram.Bytes.Length;
							
							//session.ErrorCount++;
							//session.ResendCount++;

							//if (Log.IsDebugEnabled) 
								Log.Warn($"{(datagram.RetransmitImmediate ? "NAK RSND" : "TIMEOUT")}, Resent #{datagram.Header.DatagramSequenceNumber.IntValue()} Type: {datagram.FirstMessageId} (0x{datagram.FirstMessageId:x2}) ({elapsedTime} > {datagramTimeout})");

							Interlocked.Increment(ref ConnectionInfo.Resends);
							
							SlidingWindow.OnResend(RaknetSession.CurrentTimeMillis(), datagramPair.Key);
							
							_packetSender.SendDatagram(this, datagram);
						}
					}
				}
			}
			finally
			{
				//UpdateSync.Release();
			}
		}
		private void Update()
		{
			if (Evicted) return;

			//if (MiNetServer.FastThreadPool == null) return;


			if (Evicted) return;

			ConnectionInfo.Latency = (long) SlidingWindow.GetRtt();
			long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			long lastUpdate = LastUpdatedTime.Ticks / TimeSpan.TicksPerMillisecond;

			if (lastUpdate + InactivityTimeout + 3000 < now)
			{
				Evicted = true;
				Disconnect("Network timeout.");
				Close();

				return;
			}

			if (State != ConnectionState.Connected && CustomMessageHandler != null && lastUpdate + 3000 < now)
			{
				Disconnect("Lost connection.");

				return;
			}
		}

		//private ThreadSafeList<int> _nacked = new ThreadSafeList<int>();
		private void SendNackQueue()
		{
			var           queue      = OutgoingNackQueue;
			int           queueCount = queue.Count;

			if (queueCount == 0) return;

			var acks = CustomNak.CreateObject();
			for (int i = 0; i < queueCount; i++)
			{
				if (!queue.TryDequeue(out int ack)) break;

				if (!acks.Naks.Contains(ack))
				{
					acks.Naks.Add(ack);
					Interlocked.Increment(ref ConnectionInfo.NakSent);
				}
			}

			if (acks.Naks.Count > 0)
			{
				byte[] data = acks.Encode();
				_packetSender.SendData(data, EndPoint);
				
				this.SlidingWindow.OnSendNack();
			}
			acks.PutPool();
		}
		
		private void SendAckQueue()
		{
			if (!SlidingWindow.ShouldSendAcks(CurrentTimeMillis()))
				return;
			var           queue      = OutgoingAckQueue;
			int           queueCount = queue.Count;

			if (queueCount == 0) return;

			var acks = Acks.CreateObject();
			for (int i = 0; i < queueCount; i++)
			{
				if (!queue.TryDequeue(out int ack)) break;
	//			_nacked.Remove(ack);

				if (!acks.acks.Contains(ack))
				{
					acks.acks.Add(ack);
					Interlocked.Increment(ref ConnectionInfo.AckSent);
				}
			}

			if (acks.acks.Count > 0)
			{
				byte[] data = acks.Encode();
				
				_packetSender.SendData(data, EndPoint);

				this.SlidingWindow.OnSendAck();
			}
			
			acks.PutPool();
		}

		private SemaphoreSlim _syncHack = new SemaphoreSlim(1, 1);
		public bool _bandwidthExceededStatistic = false;

		[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
		private void SendQueue(int millisecondsWait = 0)
		{
			if (_sendQueue.Count == 0) return;

			// Extremely important that this will not allow more than one thread at a time.
			// This methods handle ordering and potential encryption, hence order matters.
			//if (!(_syncHack.Wait(millisecondsWait))) return;

			try
			{
				int transmissionBandwidth;
				transmissionBandwidth = this.SlidingWindow.GetTransmissionBandwidth(this.UnackedBytes, _bandwidthExceededStatistic);

				var sendList = new List<Packet>();
				int length = _sendQueue.Count;

				for (int i = 0; i < length; i++)
				{
					if (transmissionBandwidth <= 0)
						break;
					
					Packet packet;

					if (!_sendQueue.TryDequeue(out packet))
						break;

					if (packet == null) continue;

					if (State == ConnectionState.Unconnected)
					{
						packet.PutPool();
						continue;
					}

					var packetSize = packet.Encode().Length;

					//if (packetSize < transmissionBandwidth)
					{
						transmissionBandwidth -= packetSize;
						
						sendList.Add(packet);
					}
				}

				if (sendList.Count == 0) return;

				List<Packet> prepareSend = CustomMessageHandler.PrepareSend(sendList);
				var preppedSendList = new List<Packet>();
				foreach (Packet packet in prepareSend)
				{
					Packet      message     = packet;
					
					Reliability reliability = message.ReliabilityHeader.Reliability;
					
					if (reliability == Reliability.Undefined) 
						reliability = Reliability.Reliable; // Questionable practice

					if (reliability == Reliability.ReliableOrdered || reliability == Reliability.ReliableOrderedWithAckReceipt)
						message.ReliabilityHeader.OrderingIndex = Interlocked.Increment(ref OrderingIndex);

					preppedSendList.Add(message);
					//await _packetSender.SendPacketAsync(this, message);
				}

				_packetSender.SendPacket(this, preppedSendList);
				
				_bandwidthExceededStatistic = _sendQueue.Count > 0;
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			finally
			{
			//	_syncHack.Release();
			}
		}

		public void SendDirectPacket(Packet packet)
		{
			if (packet.ReliabilityHeader.Reliability == Reliability.ReliableOrdered)
				throw new Exception($"Can't send direct messages with ordering. The offending packet was {packet.GetType().Name}");

			if (packet.ReliabilityHeader.Reliability == Reliability.Undefined)
				packet.ReliabilityHeader.Reliability = Reliability.Reliable; // Questionable practice

			_packetSender.SendPacket(this, packet);
		}

		public IPEndPoint GetClientEndPoint()
		{
			return EndPoint;
		}

		public long GetNetworkNetworkIdentifier()
		{
			return NetworkIdentifier;
		}

		private bool _closed = false;
		public void Close()
		{
			if (_closed)
				return;

			_closed = true;

			_tickerHighPrecisionTimer?.Change(Timeout.Infinite, Timeout.Infinite);
			_tickerHighPrecisionTimer?.Dispose();
			_tickerHighPrecisionTimer = null;
			
			State = ConnectionState.Unconnected;
			Evicted = true;
			
			CustomMessageHandler = null;

			// Send with high priority, bypass queue
			SendDirectPacket(DisconnectionNotification.CreateObject());

			SendQueue(500);//.Wait();

			_cancellationToken.Cancel();
			_orderingBufferQueue.Clear();

			var ackQueue = WaitingForAckQueue;
			foreach (var kvp in ackQueue)
			{
				if (ackQueue.TryRemove(kvp.Key, out Datagram datagram)) datagram.PutPool();
			}

			var splits = Splits;
			foreach (var kvp in splits)
			{
				if (splits.TryRemove(kvp.Key, out SplitPartPacket[] splitPartPackets))
				{
					if (splitPartPackets == null) continue;

					foreach (SplitPartPacket packet in splitPartPackets)
					{
						packet?.PutPool();
					}
				}
			}

			ackQueue.Clear();
			splits.Clear();

			try
			{
				_orderedQueueProcessingThread = null;
				_cancellationToken.Dispose();
			}
			catch
			{
				// ignored
			}

			if (Log.IsDebugEnabled) Log.Info($"Closed network session");
		}

		private bool   _isOutOfOrder                       = false;

		private static readonly DateTime Jan1st1970 = new DateTime
			(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		public static long CurrentTimeMillis()
		{
			return (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
		}

		public int UnackedBytes = 0;
		internal void HandleAck(Ack ack)
		{
			var queue = WaitingForAckQueue;

			foreach ((int start, int end) range in ack.ranges)
			{
				for (int i = range.start; i <= range.end; i++)
				{
					if (queue.TryRemove(i, out Datagram datagram))
					{
						Interlocked.Increment(ref ConnectionInfo.Ack);
						//_nacked.Remove(i);
						
						UnackedBytes -= datagram.Size;
						//CalculateRto(datagram);
						SlidingWindow.OnAck(CurrentTimeMillis(), datagram.Timer.ElapsedMilliseconds,i, _bandwidthExceededStatistic);
						datagram.PutPool();
					}
					else
					{
						//if (Log.IsDebugEnabled) 
						Log.Warn($"ACK, Failed to remove datagram #{i}");
					}
				}
			}
		}

		private void DoNak(int sequenceNumber)
		{
			if (WaitingForAckQueue.TryGetValue(sequenceNumber, out var datagram))
			{
				Interlocked.Increment(ref ConnectionInfo.Nak);
				SlidingWindow.OnNak(CurrentTimeMillis(), sequenceNumber);
				//CalculateRto(datagram);

				datagram.RetransmitImmediate = true;
			}
			else
			{
				//	if (Log.IsDebugEnabled)
				//Log.Warn($"NAK, no datagram #{sequenceNumber}");
			}
		}
		
		internal void HandleNak(CustomNak nak)
		{
			foreach (Tuple<int, int> range in nak.Ranges)
			{
				int start = range.Item1;
				int end = range.Item2;

				if (start == end)
				{
					DoNak(start);
					continue;
				}

				for (int i = start; i <= end; i++)
				{
					DoNak(i);
				}
			}
		}

		public bool Acknowledge(Datagram datagram)
		{
			var sequenceIndex = datagram.Header.DatagramSequenceNumber.IntValue();

			if (SlidingWindow.OnPacketReceived(
				CurrentTimeMillis(),  sequenceIndex, datagram.Size, out var skippedMessageCount))
			{
				if (skippedMessageCount > 0)
					Log.Info($"Skipped {skippedMessageCount}");
				OutgoingAckQueue.Enqueue(sequenceIndex);
				
				for (long i = skippedMessageCount; i > 0; i--)
				{
					OutgoingNackQueue.Enqueue((int) (sequenceIndex- i));
				}

				return true;
			}

			return false;
		}
	}
}