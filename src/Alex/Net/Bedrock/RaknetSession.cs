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
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Utils;
using Alex.Net.Bedrock.Raknet;
using Alex.Utils;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using NLog;
using Datagram = Alex.Net.Bedrock.Raknet.Datagram;

namespace Alex.Net.Bedrock
{
	public class RaknetSession : INetworkHandler
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		private readonly RaknetConnection _packetSender;

		private long _lastSequencingIndex = -1;
		private long _lastOrderingIndex   = -1; // That's the first message with wrapper
	//	private AutoResetEvent _packetQueuedWaitEvent = new AutoResetEvent(false);
	//	private AutoResetEvent _packetHandledWaitEvent = new AutoResetEvent(false);
		private object _eventSync    = new object();
		private object _sequenceSync = new object();

		private readonly Utils.Queue.ConcurrentPriorityQueue<Packet, int> _sequencingQueue =
			new Utils.Queue.ConcurrentPriorityQueue<Packet, int>();
		private readonly Utils.Queue.ConcurrentPriorityQueue<Packet, int> _orderingBufferQueue = new Utils.Queue.ConcurrentPriorityQueue<Packet, int>();
		private          CancellationTokenSource                          _cancellationToken;
		private          Thread                                           _orderedQueueProcessingThread;
		private          Thread                                           _sequencedQueueProcessingThread;

		public ConnectionInfo ConnectionInfo { get; }

		public ICustomMessageHandler CustomMessageHandler { get; set; }

		public string Username { get; set; }
		public IPEndPoint EndPoint { get; private set; }
		public short MtuSize { get; set; }
		public long NetworkIdentifier { get; set; }

		public int DatagramSequenceNumber = -1;
		public int ReliableMessageNumber = -1;
		public int SplitPartId = 0;
		public int OrderingIndex = -1;
		public int ErrorCount { get; set; }

		public bool Evicted { get; set; }

		public ConnectionState State { get; set; } = ConnectionState.Unconnected;

		public DateTime LastUpdatedTime { get; set; }
		public bool WaitForAck { get; set; }
		public int ResendCount { get; set; }

		/// <summary>
		/// </summary>
		public long Syn { get; set; } = 300;

		/// <summary>
		///     Round Trip Time.
		///     <code>RTT = RTT * 0.875 + rtt * 0.125</code>
		/// </summary>
		public long Rtt { get; set; } = 300;

		/// <summary>
		///     Round Trip Time Variance.
		///     <code>RTTVar = RTTVar * 0.875 + abs(RTT - rtt)) * 0.125</code>
		/// </summary>
		public long RttVar { get; set; }

		/// <summary>
		///     Retransmission Time Out.
		///     <code>RTO = RTT + 4 * RTTVar</code>
		/// </summary>
		public long Rto { get; set; }

		public int OrderingErrors = 0;

		public long InactivityTimeout { get; }
		public int ResendThreshold { get; }

		public ConcurrentDictionary<int, SplitPartPacket[]> Splits { get; } = new ConcurrentDictionary<int, SplitPartPacket[]>();
		private ConcurrentQueue<int> OutgoingAckQueue { get; } = new ConcurrentQueue<int>();
		private ConcurrentQueue<int> OutgoingNackQueue { get; } = new ConcurrentQueue<int>();
		public ConcurrentDictionary<int, Datagram> WaitingForAckQueue { get; } = new ConcurrentDictionary<int, Datagram>();

		public short CompressionThreshold { get; set; } = -1;
		public RaknetSession(ConnectionInfo connectionInfo, RaknetConnection packetSender, IPEndPoint endPoint, short mtuSize, ICustomMessageHandler messageHandler = null)
		{
			Log.Debug($"Create session for {endPoint}");

			_packetSender = packetSender;
			ConnectionInfo = connectionInfo;
			CustomMessageHandler = messageHandler ?? new DefaultMessageHandler();
			EndPoint = endPoint;
			MtuSize = mtuSize;

			InactivityTimeout = 30000;
			ResendThreshold = 10;

			_cancellationToken = new CancellationTokenSource();
		}

		/// <summary>
		///     Main receive entry to this layer. Will receive and handle messages
		///     on RakNet message level. May come from either UDP or TCP, matters not.
		/// </summary>
		/// <param name="message"></param>
		internal void HandleRakMessage(Datagram datagram, Packet message)
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
					AddToOrderedChannel(message);
					//AddToSequencedChannel(message);
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

		private void AddToSequencedChannel(Packet message)
		{
			try
			{
				if (_cancellationToken.Token.IsCancellationRequested) return;
				
				Interlocked.Exchange(ref _lastSequencingIndex, message.ReliabilityHeader.SequencingIndex);
				AddToOrderedChannel(message);
			}
			catch (Exception e)
			{
				Log.Error(e, "Something went wrong!");
			}
		}

		private ManualResetEvent _orderingResetEvent = new ManualResetEvent(false);
		private DateTime         _orderingStart      = DateTime.UtcNow;
		private void AddToOrderedChannel(Packet message)
		{
			try
			{
				if (_cancellationToken.Token.IsCancellationRequested) return;

				lock (_eventSync)
				{
					var lastOrderingIndex = Interlocked.Read(ref _lastOrderingIndex);

					if (message.ReliabilityHeader.OrderingIndex <= lastOrderingIndex)
					{
					//	return;
					}
					
					if (_orderingBufferQueue.Count == 0 && message.ReliabilityHeader.OrderingIndex == lastOrderingIndex + 1)
					{
						IsOutOfOrder = false;

						Interlocked.Exchange(ref _lastOrderingIndex, message.ReliabilityHeader.OrderingIndex);

						HandlePacket(message);
						return;
					}

					bool doOrdering = message.ReliabilityHeader.OrderingIndex == lastOrderingIndex + 1;
					if (IsOutOfOrder)
					{
						if (message.ReliabilityHeader.OrderingIndex - lastOrderingIndex > 1000) //200 packets behind should be ok
						{
							Log.Warn($"Discarded ordered packet! Index: {lastOrderingIndex + 1}");
							Interlocked.Exchange(ref _lastOrderingIndex, lastOrderingIndex + 1);
							doOrdering = true;
							IsOutOfOrder = false;
						}
					}
					else
					{
						if (message.ReliabilityHeader.OrderingIndex != lastOrderingIndex + 1)
						{
							if (!IsOutOfOrder)
							{
								IsOutOfOrder = true;
								_orderingStart = DateTime.UtcNow;

								if (Log.IsDebugEnabled)
									Log.Debug(
										$"Datagram out of order. Expected {lastOrderingIndex + 1}, but was {message.ReliabilityHeader.OrderingIndex}.");
							}
						}
					}
					
					_orderingBufferQueue.Enqueue(message, message.ReliabilityHeader.OrderingIndex);

					if (doOrdering)
					{

						_orderingResetEvent.Set();
						
						if (_orderedQueueProcessingThread == null)
							StartOrderingThread();
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
			_orderedQueueProcessingThread = new Thread(ProcessOrderedQueue)
			{
				IsBackground = true,
				Name = $"Ordering Thread [{EndPoint}]"
			};
			_orderedQueueProcessingThread.Start();
			if (Log.IsDebugEnabled) Log.Warn($"Started network ordering thread.");
		}
		
		public bool IsOutOfOrder          { get; private set; } = false;
		public int  FirstEncryptedMessage { get; set; }

		private void ProcessOrderedQueue()
		{
			try
			{
				while (!_cancellationToken.IsCancellationRequested)
				{
					while (_orderingBufferQueue.TryPeek(out KeyValuePair<int, Packet> pair) && !_cancellationToken.IsCancellationRequested)
					{
						var lastOrderingIndex = Interlocked.Read(ref _lastOrderingIndex);
						if (pair.Key == lastOrderingIndex + 1)
						{
							IsOutOfOrder = false;
							if (_orderingBufferQueue.TryDequeue(out pair))
							{
								Interlocked.Exchange(ref _lastOrderingIndex, pair.Key);
								
								HandlePacket(pair.Value);
							}
						}
						else if (pair.Key <= lastOrderingIndex)
						{
							if (Log.IsDebugEnabled) Log.Debug($"Datagram resent. Expected {lastOrderingIndex + 1}, but was {pair.Key}.");
							if (_orderingBufferQueue.TryDequeue(out pair))
							{
								pair.Value.PutPool();
							}
						}
					}

					if (!_orderingResetEvent.WaitOne(500)) //Keep the thread alive for longer.
						return;
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

			_orderedQueueProcessingThread = null;
		}
		
		private void StartSequencingThread()
		{
			_sequencedQueueProcessingThread = new Thread(ProcessSequenceQueue)
			{
				IsBackground = true,
				Name = $"Sequencing Thread [{EndPoint}]"
			};
			_sequencedQueueProcessingThread.Start();
			if (Log.IsDebugEnabled) Log.Warn($"Started network sequencing thread.");
		}
		
		private void ProcessSequenceQueue()
		{
			try
			{
				while (_sequencingQueue.TryPeek(out KeyValuePair<int, Packet> pair)
				       && !_cancellationToken.IsCancellationRequested)
				{
					var lastSequenceIndex = Interlocked.Read(ref _lastSequencingIndex);

					if (pair.Key == lastSequenceIndex + 1)
					{
						IsOutOfOrder = false;

						if (_sequencingQueue.TryDequeue(out pair))
						{
							Interlocked.Exchange(ref _lastSequencingIndex, pair.Key);
							AddToOrderedChannel(pair.Value);
						}
					}
					else if (pair.Key <= lastSequenceIndex)
					{
						if (Log.IsDebugEnabled)
							Log.Debug($"Datagram resent. Expected {lastSequenceIndex + 1}, but was {pair.Key}.");

						if (_sequencingQueue.TryDequeue(out pair))
						{
							pair.Value.PutPool();
						}
					}
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

			_sequencedQueueProcessingThread = null;
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

				if (message.Id < (int) DefaultMessageIdTypes.ID_USER_PACKET_ENUM)
				{
					// Standard RakNet online message handlers
					switch (message)
					{
						case ConnectedPing connectedPing:
							HandleConnectedPing(connectedPing);
							break;
						case ConnectedPong connectedPong:
							HandleConnectedPong(connectedPong);
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
							Log.Error($"Unhandled packet: {message.GetType().Name} 0x{message.Id:X2} for user: {Username}, IP {EndPoint.Address}");
							if (Log.IsDebugEnabled) Log.Warn($"Unknown packet 0x{message.Id:X2}\n{Packet.HexDump(message.Bytes)}");
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
						Log.Warn($"Packet (0x{message.Id:x2}) handling too long {elapsedMilliseconds}ms ({message.ToString()})");
					}
				}
				else
				{
					Log.Warn("Packet (0x{0:x2}) timer not started for {1}.", message.Id, Username);
				}
			}
			catch (Exception e)
			{
				Log.Error(e,"Packet handling");
				throw;
			}
			finally
			{
				message?.PutPool();
			}
		}

		private long _pongsReceived    = 0;
		private long _totalLatency = 0;
		private void HandleConnectedPong(ConnectedPong connectedPong)
		{
			var now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			// Ignore
			if (_pings.Remove(connectedPong.sendpingtime))
			{
				long responseTime = now - connectedPong.sendpingtime;
				//_totalLatency += responseTime;

				ConnectionInfo.Latency = responseTime; //_totalLatency / ++_pongsReceived;
			}

			foreach (var ping in _pings.ToArray())
			{
				long responseTime = now - ping;

				if (responseTime > 2500)
					_pings.Remove(ping);
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

		private DateTime             _lastDetectionSendTime = DateTime.UtcNow;
		private DateTime             _lastPingTIme          = DateTime.UtcNow;
		private ThreadSafeList<long> _pings                 = new ThreadSafeList<long>();
		public void DetectLostConnection()
		{
			long now        = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			long lastUpdate = _lastDetectionSendTime.Ticks / TimeSpan.TicksPerMillisecond;

			if (now - lastUpdate > 2500)
			{
				var ping = DetectLostConnections.CreateObject();
				//ping.ReliabilityHeader.Reliability = Reliability.Unreliable;
				SendPacket(ping);

				_lastDetectionSendTime = DateTime.UtcNow;
			}
		}

		// MCPE Login handling


		private ConcurrentQueue<Packet> _sendQueue = new ConcurrentQueue<Packet>();
	//	private object                  _queueSync              = new object();

		public void SendPacket(Packet packet)
		{
			if (packet == null) return;

			if (State == ConnectionState.Unconnected)
			{
				if (Log.IsDebugEnabled) Log.Debug($"Ignoring send of packet {packet.GetType().Name} because session is not connected");
				packet.PutPool();
				return;
			}

		//	RakOfflineHandler.TraceSend(packet);

			//lock (_queueSync)
			{
				_sendQueue.Enqueue(packet);
			}
		}

		private int _tickCounter;

		public async Task SendTickAsync(global::Alex.Net.Bedrock.RaknetConnection connection)
		{
			try
			{
				if (_tickCounter++ >= 5)
				{
					await Task.WhenAll(SendAckQueueAsync(), SendNackQueueAsync(), UpdateAsync(), SendQueueAsync(), connection.UpdateAsync(this));
					_tickCounter = 0;
				}
				else
				{
					await Task.WhenAll(SendAckQueueAsync(), SendNackQueueAsync(), SendQueueAsync());
				}
			}
			catch (Exception e)
			{
				Log.Warn(e);
			}
		}


		//private object _updateSync = new object();
		private SemaphoreSlim _updateSync = new SemaphoreSlim(1, 1);

		private async Task UpdateAsync()
		{
			if (Evicted) return;

			//if (MiNetServer.FastThreadPool == null) return;

			if (!await _updateSync.WaitAsync(0)) return;

			try
			{
				if (Evicted) return;

				long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
				long lastUpdate = LastUpdatedTime.Ticks / TimeSpan.TicksPerMillisecond;

				if (lastUpdate + InactivityTimeout + 3000 < now)
				{
					Evicted = true;
					// Disconnect user
					//ThreadPool.QueueUserWorkItem(o =>
					{
						Disconnect("Network timeout.");
						Close();
					}//);

					return;
				}

				if (State != ConnectionState.Connected && CustomMessageHandler != null && lastUpdate + 3000 < now)
				{
					Disconnect("Lost connection."); 
					return;
				}

				if (State == ConnectionState.Connected)
				{
					lastUpdate = _lastPingTIme.Ticks / TimeSpan.TicksPerMillisecond;

					if (now - lastUpdate > 2500)
					{
						ConnectedPing connectedPing = ConnectedPing.CreateObject();
						connectedPing.sendpingtime = now;
						//connectedPing.ReliabilityHeader.Reliability = Reliability.Unreliable;

						SendPacket(connectedPing);

						_pings.TryAdd(connectedPing.sendpingtime);
						_lastPingTIme = DateTime.UtcNow;
					}
				}
			}
			finally
			{
				_updateSync.Release();
			}
		}

		private async Task SendNackQueueAsync()
		{
			RaknetSession session    = this;
			var           queue      = session.OutgoingNackQueue;
			int           queueCount = queue.Count;

			if (queueCount == 0) return;

			var acks = new CustomNak();
			for (int i = 0; i < queueCount; i++)
			{
				if (!queue.TryDequeue(out int ack)) break;

				ConnectionInfo.NumberOfPlayers++;
				acks.naks.Add(ack);
			}

			if (acks.naks.Count > 0)
			{
				byte[] data = acks.Encode();
				await _packetSender.SendDataAsync(data, session.EndPoint);
				
			}
			//acks.PutPool();
		}
		
		private async Task SendAckQueueAsync()
		{
			RaknetSession session    = this;
			var           queue      = session.OutgoingAckQueue;
			int           queueCount = queue.Count;

			if (queueCount == 0) return;

			var acks = new Acks();
			for (int i = 0; i < queueCount; i++)
			{
				if (!queue.TryDequeue(out int ack)) break;

				Interlocked.Increment(ref ConnectionInfo.NumberOfAckSent);
				acks.acks.Add(ack);
			}

			if (acks.acks.Count > 0)
			{
				byte[] data = acks.Encode();
				await _packetSender.SendDataAsync(data, session.EndPoint);
			}
			//acks.PutPool();
		}

		private SemaphoreSlim _syncHack = new SemaphoreSlim(1, 1);

		[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
		private async Task SendQueueAsync(int millisecondsWait = 0)
		{
			if (_sendQueue.Count == 0) return;

			// Extremely important that this will not allow more than one thread at a time.
			// This methods handle ordering and potential encryption, hence order matters.
			if (!(await _syncHack.WaitAsync(millisecondsWait))) return;

			try
			{
				var sendList = new List<Packet>();
				//Queue<Packet> queue = _sendQueueNotConcurrent;
				int length = _sendQueue.Count;
				for (int i = 0; i < length; i++)
				{
					Packet packet;

					if (!_sendQueue.TryDequeue(out packet))
						break;

					if (packet == null) continue;

					if (State == ConnectionState.Unconnected)
					{
						packet.PutPool();
						continue;
					}

					sendList.Add(packet);
				}

				if (sendList.Count == 0) return;

				List<Packet> prepareSend = CustomMessageHandler.PrepareSend(sendList);
				var preppedSendList = new List<Packet>();
				foreach (Packet packet in prepareSend)
				{
					Packet message = packet;

					if (CustomMessageHandler != null) message = CustomMessageHandler.HandleOrderedSend(message);

					Reliability reliability = message.ReliabilityHeader.Reliability;
					if (reliability == Reliability.Undefined) reliability = Reliability.Reliable; // Questionable practice

					if (reliability == Reliability.ReliableOrdered) message.ReliabilityHeader.OrderingIndex = Interlocked.Increment(ref OrderingIndex);

					preppedSendList.Add(message);
					//await _packetSender.SendPacketAsync(this, message);
				}

				await _packetSender.SendPacketAsync(this, preppedSendList);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
			finally
			{
				_syncHack.Release();
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

		public void Close()
		{
			State = ConnectionState.Unconnected;
			Evicted = true;
			
			CustomMessageHandler = null;

			// Send with high priority, bypass queue
			SendDirectPacket(DisconnectionNotification.CreateObject());

			SendQueueAsync(500).Wait();

			_cancellationToken.Cancel();
		//	_packetQueuedWaitEvent.Set();
		//	_packetHandledWaitEvent.Set();
			_orderingBufferQueue.Clear();

			_packetSender.Close(this);

			try
			{
				_orderedQueueProcessingThread = null;
				_cancellationToken.Dispose();
			//	_packetQueuedWaitEvent.Close();
			//	_packetHandledWaitEvent.Close();
			}
			catch
			{
				// ignored
			}

			if (Log.IsDebugEnabled) Log.Info($"Closed network session for player {Username}");
		}
		
		private long                 _lastDatagramSequenceNumber = -1;
		public void Acknowledge(Int24 datagramSequenceNumber)
		{
			var integerValue = datagramSequenceNumber.IntValue();
			var last         = Interlocked.Exchange(ref _lastDatagramSequenceNumber, integerValue);
			var skipped      = last - integerValue - 1;

			if (skipped > 0)
			{
				for (long i = last + 1; i < integerValue - 1; i++)
				{
					OutgoingNackQueue.Enqueue((int) i);
				}
			}

			OutgoingAckQueue.Enqueue(datagramSequenceNumber);
		}
	}
}