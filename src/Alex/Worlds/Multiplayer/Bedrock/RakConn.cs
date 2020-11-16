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
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class RakConnection
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(RakConnection));

		private          UdpClient  _listener;
		private readonly IPEndPoint _endpoint;

		private Thread _receiveThread;
		private HighPrecisionTimer _tickerHighPrecisionTimer;
		public readonly ConcurrentDictionary<IPEndPoint, RakSession> RakSessions = new ConcurrentDictionary<IPEndPoint, RakSession>();

		public readonly RakOfflineHandler RakOfflineHandler;
		public ConnectionInfo ConnectionInfo { get; }

		public bool FoundServer => RakOfflineHandler.HaveServer;

		public bool AutoConnect
		{
			get => RakOfflineHandler.AutoConnect;
			set => RakOfflineHandler.AutoConnect = value;
		}

		// This is only used in client scenarios. Will contain
		// information regarding a located server.
		public IPEndPoint RemoteEndpoint { get; set; }
		public string RemoteServerName { get; set; }

		public Func<RakSession, ICustomMessageHandler> CustomMessageHandlerFactory { get; set; }
		
		public RakConnection()
		{
			_endpoint = new IPEndPoint(IPAddress.Any, 0);

			ConnectionInfo = new ConnectionInfo(new ConcurrentDictionary<IPEndPoint, MiNET.Net.RakNet.RakSession>());

			RakOfflineHandler = new RakOfflineHandler(this, ConnectionInfo);
		}

		public void Start()
		{
			if (_listener != null) return;

			Log.Debug($"Creating listener for packets on {_endpoint}");
			_listener = CreateListener(_endpoint);
			_receiveThread = new Thread(ReceiveDatagram) {IsBackground = true, Priority = ThreadPriority.AboveNormal};
			_receiveThread.Start(_listener);

			_tickerHighPrecisionTimer = new HighPrecisionTimer(10, SendTick, true);
		}

		public bool TryLocate(out (IPEndPoint serverEndPoint, string serverName) serverInfo, int numberOfAttempts = int.MaxValue)
		{
			return TryLocate(null, out serverInfo, numberOfAttempts);
		}

		public bool TryLocate(IPEndPoint targetEndPoint, out (IPEndPoint serverEndPoint, string serverName) serverInfo, int numberOfAttempts = int.MaxValue)
		{
			Start(); // Make sure we have started.

			bool oldAutoConnect = AutoConnect;
			AutoConnect = false;

			while (!FoundServer && numberOfAttempts-- > 0)
			{
				SendUnconnectedPingInternal(targetEndPoint);
				Task.Delay(100).Wait();
			}

			serverInfo = (RemoteEndpoint, RemoteServerName);

			AutoConnect = oldAutoConnect;

			return FoundServer;
		}


		public bool TryConnect(IPEndPoint targetEndPoint, int numberOfAttempts = int.MaxValue, short mtuSize = 1500)
		{
			Start(); // Make sure we have started the listener

			RakSession session;
			do
			{
				RakOfflineHandler.SendOpenConnectionRequest1(targetEndPoint, mtuSize);
				Task.Delay(300).Wait();
			} while ((!RakSessions.TryGetValue(targetEndPoint, out session) && RakSessions.Count < 0) && numberOfAttempts-- > 0);

			if (session == null) return false;

			while (session.State != ConnectionState.Connected && numberOfAttempts-- > 0)
			{
				Task.Delay(100).Wait();
			}

			return session.State == ConnectionState.Connected;
		}

		private void SendUnconnectedPingInternal(IPEndPoint targetEndPoint)
		{
			var packet = new UnconnectedPing
			{
				pingId = Stopwatch.GetTimestamp() /*incoming.pingId*/,
				guid = RakOfflineHandler.ClientGuid
			};

			var data = packet.Encode();

			if (targetEndPoint != null)
			{
				SendData(data, targetEndPoint);
			}
			else
			{
				SendData(data, new IPEndPoint(IPAddress.Broadcast, 19132));
			}
		}

		public void Stop()
		{
			try
			{
				//Log.Info("Shutting down...");

				foreach (var rakSession in RakSessions)
				{
					rakSession.Value.Close();
				}

				var timer = _tickerHighPrecisionTimer;
				_tickerHighPrecisionTimer = null;
				timer?.Dispose();

				var listener = _listener;
				if (listener == null) return;

				_listener = null;
				listener.Close();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}


		private static UdpClient CreateListener(IPEndPoint endpoint)
		{
			var listener = new UdpClient();

			if (Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				//_listener.Client.ReceiveBufferSize = 1600*40000;
				//listener.Client.ReceiveBufferSize = int.MaxValue;
				//_listener.Client.SendBufferSize = 1600*40000;
				//listener.Client.SendBufferSize = int.MaxValue;
			}

			listener.DontFragment = true;
			listener.EnableBroadcast = false;

			if (Environment.OSVersion.Platform != PlatformID.Unix && Environment.OSVersion.Platform != PlatformID.MacOSX)
			{
				// SIO_UDP_CONNRESET (opcode setting: I, T==3)
				// Windows:  Controls whether UDP PORT_UNREACHABLE messages are reported.
				// - Set to TRUE to enable reporting.
				// - Set to FALSE to disable reporting.

				uint IOC_IN = 0x80000000;
				uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				listener.Client.IOControl((int) SIO_UDP_CONNRESET, new byte[] {Convert.ToByte(false)}, null);

				//
				//WARNING: We need to catch errors here to remove the code above.
				//
			}

		//	listener.
			listener.ExclusiveAddressUse = true;
			listener.Client.Bind(endpoint);
			return listener;
		}

		public void Close(RakSession session)
		{
			var ackQueue = session.WaitingForAckQueue;
			foreach (var kvp in ackQueue)
			{
				if (ackQueue.TryRemove(kvp.Key, out Datagram datagram)) datagram.PutPool();
			}

			var splits = session.Splits;
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
		}


		private void ReceiveDatagram(object state)
		{
			var listener = (UdpClient) state;

			while (true)
			{
				// Check if we already closed the server
				if (listener?.Client == null) return;

				// WSAECONNRESET:
				// The virtual circuit was reset by the remote side executing a hard or abortive close. 
				// The application should close the socket; it is no longer usable. On a UDP-datagram socket 
				// this error indicates a previous send operation resulted in an ICMP Port Unreachable message.
				// Note the spocket settings on creation of the server. It makes us ignore these resets.
				IPEndPoint senderEndpoint = null;
				try
				{
					var receiveBytes = listener.Receive(ref senderEndpoint);
					if (receiveBytes.Length == 0)
						continue;
					//UdpReceiveResult result = listener.ReceiveAsync().Result;
					//senderEndpoint = result.RemoteEndPoint;
					//byte[] receiveBytes = result.Buffer;

					if (receiveBytes.Length != 0)
					{
						Interlocked.Increment(ref ConnectionInfo.NumberOfPacketsInPerSecond);
						Interlocked.Add(ref ConnectionInfo.TotalPacketSizeInPerSecond, receiveBytes.Length);
						
						//ThreadPool.QueueUserWorkItem(p =>
						{
							try
							{
								ReceiveDatagram(receiveBytes, senderEndpoint);
							}
							catch (Exception e)
							{
								Log.Warn($"Process message error from: {senderEndpoint.Address}", e);
							}
						}//);
					}
					else
					{
						Log.Warn("Unexpected end of transmission?");
						continue;
					}
				}
				catch (ObjectDisposedException e)
				{
					return;
				}
				catch (SocketException e)
				{
					// 10058 (just regular disconnect while listening)
					if (e.ErrorCode == 10058) return;
					if (e.ErrorCode == 10038) return;
					if (e.ErrorCode == 10004) return;

					if (Log.IsDebugEnabled) Log.Error("Unexpected end of receive", e);

					if (listener.Client != null) continue; // ??

					return;
				}
			}
		}

		private void ReceiveDatagram(ReadOnlyMemory<byte> receivedBytes, IPEndPoint clientEndpoint)
		{
			var header = new InternalDatagramHeader(receivedBytes.Span[0]);

			if (!header.IsValid)
			{
				// We parse as an offline message. This is not actually correct, but works.

				byte messageId = receivedBytes.Span[0];

				if (messageId <= (byte) DefaultMessageIdTypes.ID_USER_PACKET_ENUM)
				{
					RakOfflineHandler.HandleOfflineRakMessage(receivedBytes, clientEndpoint);
				}
				else
				{
					Log.Warn($"Receive invalid message, but not a RakNet message. Message ID={messageId}. Ignoring.");
				}

				return;
			}

			if (!RakSessions.TryGetValue(clientEndpoint, out RakSession rakSession))
			{
				return;
			}

			if (rakSession.Evicted) return;

			rakSession.LastUpdatedTime = DateTime.UtcNow;
			
			if (header.IsAck)
			{
				var ack = new Ack();
				ack.Decode(receivedBytes);

				HandleAck(rakSession, ack, ConnectionInfo);
				return;
			}
			
			if (header.IsNak)
			{
				var nak = new Nak();
				nak.Decode(receivedBytes);

				HandleNak(rakSession, nak, ConnectionInfo);
				return;
			}

			var datagram = Datagram.CreateObject();
			try
			{
				datagram.Decode(receivedBytes);
			}
			catch (Exception e)
			{
				rakSession.Disconnect("Bad packet received from client.");

				Log.Warn($"Bad packet {receivedBytes.Span[0]}\n{Packet.HexDump(receivedBytes)}", e);
				return;
			}

			EnqueueAck(rakSession, datagram.Header.DatagramSequenceNumber);

			if (Log.IsVerboseEnabled()) Log.Verbose($"Receive datagram #{datagram.Header.DatagramSequenceNumber} for {_endpoint}");

			HandleDatagram(rakSession, datagram);
			datagram.PutPool();
		}

		private void HandleDatagram(RakSession session, Datagram datagram)
		{
			foreach (Packet packet in datagram.Messages)
			{
				Packet message = packet;
				if (message is SplitPartPacket splitPartPacket)
				{
					message = HandleSplitMessage(session, splitPartPacket);
					if (message == null) continue;
				}

				message.Timer.Restart();
				session.HandleRakMessage(message);
			}
		}

		private Packet HandleSplitMessage(RakSession session, SplitPartPacket splitPart)
		{
			int spId = splitPart.ReliabilityHeader.PartId;
			int spIdx = splitPart.ReliabilityHeader.PartIndex;
			int spCount = splitPart.ReliabilityHeader.PartCount;

			SplitPartPacket[] splitPartList = session.Splits.GetOrAdd(spId, new SplitPartPacket[spCount]);
			bool haveAllParts = true;
			// Need sync for this part since they come very fast, and very close in time. 
			// If no sync, will often detect complete message two times (or more).

			lock (splitPartList)
			{
				// Already had part (resent). Then ignore. 
				if (splitPartList[spIdx] != null) return null;

				splitPartList[spIdx] = splitPart;

				foreach (SplitPartPacket spp in splitPartList)
				{
					if (spp != null) continue;

					haveAllParts = false;
					break;
				}
			}

			if (!haveAllParts) return null;

			if (Log.IsVerboseEnabled()) Log.Verbose($"Got all {spCount} split packets for split ID: {spId}");

			session.Splits.TryRemove(spId, out SplitPartPacket[] _);

			int contiguousLength = 0;
			foreach (SplitPartPacket spp in splitPartList)
			{
				contiguousLength += spp.Message.Length;
			}

			var buffer = new Memory<byte>(new byte[contiguousLength]);

			Reliability headerReliability = splitPart.ReliabilityHeader.Reliability;
			var headerReliableMessageNumber = splitPart.ReliabilityHeader.ReliableMessageNumber;
			var headerOrderingChannel = splitPart.ReliabilityHeader.OrderingChannel;
			var headerOrderingIndex = splitPart.ReliabilityHeader.OrderingIndex;

			int position = 0;
			foreach (SplitPartPacket spp in splitPartList)
			{
				spp.Message.CopyTo(buffer.Slice(position));
				position += spp.Message.Length;
				spp.PutPool();
			}

			try
			{
				Packet fullMessage = PacketFactory.Create(buffer.Span[0], buffer, "raknet") ??
									new UnknownPacket(buffer.Span[0], buffer.ToArray());

				fullMessage.ReliabilityHeader = new ReliabilityHeader()
				{
					Reliability = headerReliability,
					ReliableMessageNumber = headerReliableMessageNumber,
					OrderingChannel = headerOrderingChannel,
					OrderingIndex = headerOrderingIndex,
				};

				if (Log.IsVerboseEnabled()) Log.Verbose($"Assembled split packet {fullMessage.ReliabilityHeader.Reliability} message #{fullMessage.ReliabilityHeader.ReliableMessageNumber}, OrdIdx: #{fullMessage.ReliabilityHeader.OrderingIndex}");

				return fullMessage;
			}
			catch (Exception e)
			{
				Log.Error("Error during split message parsing", e);
				if (Log.IsDebugEnabled) Log.Debug($"0x{buffer.Span[0]:x2}\n{Packet.HexDump(buffer)}");
				session.Disconnect("Bad packet received from client.", false);
			}

			return null;
		}


		private void EnqueueAck(RakSession session, Int24 datagramSequenceNumber)
		{
			session.OutgoingAckQueue.Enqueue(datagramSequenceNumber);
		}

		private void HandleAck(RakSession session, Ack ack, ConnectionInfo connectionInfo)
		{
			var queue = session.WaitingForAckQueue;

			foreach ((int start, int end) range in ack.ranges)
			{
				Interlocked.Increment(ref connectionInfo.NumberOfAckReceive);

				for (int i = range.start; i <= range.end; i++)
				{
					if (queue.TryRemove(i, out Datagram datagram))
					{
						CalculateRto(session, datagram);

						datagram.PutPool();
					}
					else
					{
						if (Log.IsDebugEnabled) Log.Warn($"ACK, Failed to remove datagram #{i} for {session.Username}. Queue size={queue.Count}");
					}
				}
			}

			session.ResendCount = 0;
			session.WaitForAck = false;
		}

		internal void HandleNak(RakSession session, Nak nak, ConnectionInfo connectionInfo)
		{
			var queue = session.WaitingForAckQueue;

			foreach (Tuple<int, int> range in nak.ranges)
			{
				Interlocked.Increment(ref connectionInfo.NumberOfNakReceive);

				int start = range.Item1;
				int end = range.Item2;

				for (int i = start; i <= end; i++)
				{
					if (queue.TryGetValue(i, out var datagram))
					{
						CalculateRto(session, datagram);

						datagram.RetransmitImmediate = true;
					}
					else
					{
						if (Log.IsDebugEnabled)
							Log.WarnFormat("NAK, no datagram #{0} for {1}", i, session.Username);
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CalculateRto(RakSession session, Datagram datagram)
		{
			// RTT = RTT * 0.875 + rtt * 0.125
			// RTTVar = RTTVar * 0.875 + abs(RTT - rtt)) * 0.125
			// RTO = RTT + 4 * RTTVar
			long rtt = datagram.Timer.ElapsedMilliseconds;
			long RTT = session.Rtt;
			long RTTVar = session.RttVar;

			session.Rtt = (long) (RTT * 0.875 + rtt * 0.125);
			session.RttVar = (long) (RTTVar * 0.875 + Math.Abs(RTT - rtt) * 0.125);
			session.Rto = session.Rtt + 4 * session.RttVar + 100; // SYNC time in the end
		}

		private async void SendTick(object obj)
		{
			var tasks = new List<Task>();
			foreach (KeyValuePair<IPEndPoint, RakSession> session in RakSessions)
			{
				tasks.Add(session.Value.SendTickAsync(this));
			}
			await Task.WhenAll(tasks);
		}

		internal async Task UpdateAsync(RakSession session)
		{
			if (session.Evicted) return;

			try
			{
				long now = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
				long lastUpdate = session.LastUpdatedTime.Ticks / TimeSpan.TicksPerMillisecond;

				if (!session.WaitForAck && (session.ResendCount > session.ResendThreshold || lastUpdate + session.InactivityTimeout < now))
				{
					//TODO: Seems to have lost code here. This should actually count the resends too.
					// Spam is a bit too much. The Russians have trouble with bad connections.
					session.DetectLostConnection();
					session.WaitForAck = true;
				}

				if (session.WaitingForAckQueue.Count == 0) return;

				if (session.WaitForAck) return;

				if (session.Rto == 0) return;

				long rto = Math.Max(100, session.Rto);
				var queue = session.WaitingForAckQueue;

				foreach (KeyValuePair<int, Datagram> datagramPair in queue)
				{
					if (session.Evicted) return;

					Datagram datagram = datagramPair.Value;

					if (!datagram.Timer.IsRunning)
					{
						Log.Error($"Timer not running for #{datagram.Header.DatagramSequenceNumber}");
						datagram.Timer.Restart();
						continue;
					}

					if (session.Rtt == -1) return;

					long elapsedTime = datagram.Timer.ElapsedMilliseconds;
					long datagramTimeout = rto * (datagram.TransmissionCount + session.ResendCount + 1);
					datagramTimeout = Math.Min(datagramTimeout, 3000);
					datagramTimeout = Math.Max(datagramTimeout, 100);

					if (datagram.RetransmitImmediate || elapsedTime >= datagramTimeout)
					{
						if (!session.Evicted && session.WaitingForAckQueue.TryRemove(datagram.Header.DatagramSequenceNumber, out _))
						{
							session.ErrorCount++;
							session.ResendCount++;

							if (Log.IsDebugEnabled) 
								Log.Warn($"{(datagram.RetransmitImmediate ? "NAK RSND" : "TIMEOUT")}, Resent #{datagram.Header.DatagramSequenceNumber.IntValue()} Type: {datagram.FirstMessageId} (0x{datagram.FirstMessageId:x2}) for {session.Username} ({elapsedTime} > {datagramTimeout}) RTO {session.Rto}");

							Interlocked.Increment(ref ConnectionInfo.NumberOfResends);
							await SendDatagramAsync(session, datagram);
						}
					}
				}
			}
			finally
			{
				//session._updateSync.Release();
			}
		}

		public async Task SendPacketAsync(RakSession session, Packet message)
		{
			foreach (Datagram datagram in Datagram.CreateDatagrams(message, session.MtuSize, session))
			{
				await SendDatagramAsync(session, datagram);
			}

			message.PutPool();
		}

		public async Task SendPacketAsync(RakSession session, List<Packet> messages)
		{
			foreach (Datagram datagram in Datagram.CreateDatagrams(messages, session.MtuSize, session))
			{
				await SendDatagramAsync(session, datagram);
			}

			foreach (Packet message in messages)
			{
				message.PutPool();
			}
		}


		public async Task SendDatagramAsync(RakSession session, Datagram datagram)
		{
			if (datagram.MessageParts.Count == 0)
			{
				Log.Warn($"Failed to send #{datagram.Header.DatagramSequenceNumber.IntValue()}");
				datagram.PutPool();
				return;
			}

			if (datagram.TransmissionCount > 10)
			{
				if (Log.IsDebugEnabled) Log.Warn($"Retransmission count exceeded. No more resend of #{datagram.Header.DatagramSequenceNumber.IntValue()} Type: {datagram.FirstMessageId} (0x{datagram.FirstMessageId:x2}) for {session.Username}");

				datagram.PutPool();

				Interlocked.Increment(ref ConnectionInfo.NumberOfFails);
				//TODO: Disconnect! Because of encryption, this connection can't be used after this point
				return;
			}

			datagram.Header.DatagramSequenceNumber = Interlocked.Increment(ref session.DatagramSequenceNumber);
			datagram.TransmissionCount++;
			datagram.RetransmitImmediate = false;

			byte[] buffer = new byte[1600];;// ArrayPool<byte>.Shared.Rent(1600);
			int length = (int) datagram.GetEncoded(ref buffer);

			datagram.Timer.Restart();

			if (!ConnectionInfo.DisableAck && !ConnectionInfo.IsEmulator && !session.WaitingForAckQueue.TryAdd(datagram.Header.DatagramSequenceNumber.IntValue(), datagram))
			{
				Log.Warn($"Datagram sequence unexpectedly existed in the ACK/NAK queue already {datagram.Header.DatagramSequenceNumber.IntValue()}");
				datagram.PutPool();
			}

			//lock (session.SyncRoot)
			{
				await SendDataAsync(buffer, length, session.EndPoint);
				//ArrayPool<byte>.Shared.Return(buffer);
			}
		}


		public void SendData(byte[] data, IPEndPoint targetEndPoint)
		{
			try
			{
				_listener.Send(data, data.Length, targetEndPoint); // Less thread-issues it seems

				Interlocked.Increment(ref ConnectionInfo.NumberOfPacketsOutPerSecond);
				Interlocked.Add(ref ConnectionInfo.TotalPacketSizeOutPerSecond, data.Length);
			}
			catch (ObjectDisposedException e)
			{
			}
			catch (Exception e)
			{
				//if (_listener == null || _listener.Client != null) Log.Error(string.Format("Send data lenght: {0}", data.Length), e);
			}
		}

		public async Task SendDataAsync(byte[] data, IPEndPoint targetEndPoint)
		{
			await SendDataAsync(data, data.Length, targetEndPoint);
		}

		public async Task SendDataAsync(byte[] data, int length, IPEndPoint targetEndPoint)
		{
			try
			{
				await _listener.SendAsync(data, length, targetEndPoint);
				//_listener.Send(data, length, targetEndPoint);

				Interlocked.Increment(ref ConnectionInfo.NumberOfPacketsOutPerSecond);
				Interlocked.Add(ref ConnectionInfo.TotalPacketSizeOutPerSecond, length);
			}
			catch (ObjectDisposedException e)
			{
				Log.Warn(e);
			}
			catch (Exception e)
			{
				Log.Warn(e);
				//if (_listener == null || _listener.Client != null) Log.Error(string.Format("Send data lenght: {0}", data.Length), e);
			}
		}
	}
}