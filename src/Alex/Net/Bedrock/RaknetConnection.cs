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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Alex.Net.Bedrock.Raknet;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using NLog;
using Datagram = Alex.Net.Bedrock.Raknet.Datagram;

namespace Alex.Net.Bedrock
{
	public class RaknetConnection
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		private          UdpClient  _listener;
		private readonly IPEndPoint _endpoint;
		
		private HighPrecisionTimer _tickerHighPrecisionTimer;

		private Thread _readingThread;

		public          RaknetSession  Session { get; set; } = null;
		public readonly RaknetHandler  RaknetHandler;
		public   ConnectionInfo ConnectionInfo { get; }

		public bool FoundServer => RaknetHandler.HaveServer;

		public bool AutoConnect
		{
			get => RaknetHandler.AutoConnect;
			set => RaknetHandler.AutoConnect = value;
		}

		public bool IsNetworkOutOfOrder => Session?.IsOutOfOrder ?? false;

		// This is only used in client scenarios. Will contain
		// information regarding a located server.
		public IPEndPoint RemoteEndpoint { get; set; }
		public string RemoteServerName { get; set; }

		public Func<RaknetSession, ICustomMessageHandler> CustomMessageHandlerFactory { get; set; }

		public RaknetConnection()
		{
			_endpoint = new IPEndPoint(IPAddress.Any, 0);

			ConnectionInfo = new ConnectionInfo(new ConcurrentDictionary<IPEndPoint, MiNET.Net.RakNet.RakSession>());

			RaknetHandler = new RaknetHandler(this, ConnectionInfo);
		}

		public void Start()
		{
			if (_listener != null) return;

			_readingThread = new Thread(ReceiveCallback);
			
			_listener = CreateListener(_endpoint);
			_readingThread.Start();

			_tickerHighPrecisionTimer = new HighPrecisionTimer(10, SendTick, true);
		}

		public bool TryConnect(IPEndPoint targetEndPoint, int numberOfAttempts = int.MaxValue, short mtuSize = 1500)
		{
			Start(); // Make sure we have started the listener
			
			do
			{
				RaknetHandler.SendOpenConnectionRequest1(targetEndPoint, mtuSize);
				Task.Delay(300).Wait();
			} while (Session == null && numberOfAttempts-- > 0);

			if (Session == null) return false;

			while (Session.State != ConnectionState.Connected && numberOfAttempts-- > 0)
			{
				Task.Delay(100).Wait();
			}

			return Session.State == ConnectionState.Connected;
		}

		public void Stop()
		{
			try
			{
				//Log.Info("Shutting down...");
				Session?.Close();

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
				listener.Client.ReceiveBufferSize = int.MaxValue;
				//_listener.Client.SendBufferSize = 1600*40000;
				listener.Client.SendBufferSize = int.MaxValue;
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
			//listener.ExclusiveAddressUse = true;
			listener.Client.Bind(endpoint);
			return listener;
		}

		public void Close(RaknetSession session)
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

		private async void ReceiveCallback(object o)
		{
			while (_listener != null)
			{
				var listener = _listener;

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
					var receive      = await listener.ReceiveAsync();
					var receiveBytes = receive.Buffer;
					senderEndpoint = receive.RemoteEndPoint;
					
					Interlocked.Increment(ref ConnectionInfo.NumberOfPacketsInPerSecond);
					Interlocked.Add(ref ConnectionInfo.TotalPacketSizeInPerSecond, receiveBytes.Length);

					if (receiveBytes.Length != 0)
					{
						//Log.Info($"Buffer size: {receiveBytes.Length}");
						//ThreadPool.QueueUserWorkItem(
						//	(o) =>
						//{
							try
							{
								ReceiveDatagram(receiveBytes, senderEndpoint);
							}
							catch (Exception e)
							{
								Log.Warn(e, $"Process message error from: {senderEndpoint.Address}");
							}
						//} );
					}
					else
					{
						Log.Warn("Unexpected end of transmission?");
					}

					//listener.BeginReceive(ReceiveCallback, listener);
				}
				catch (ObjectDisposedException e) { }
				catch (SocketException e)
				{
					// 10058 (just regular disconnect while listening)
					if (e.ErrorCode == 10058) return;
					if (e.ErrorCode == 10038) return;
					if (e.ErrorCode == 10004) return;

					if (Log.IsDebugEnabled) Log.Error("Unexpected end of receive", e);
				}
				catch (NullReferenceException ex)
				{
					Log.Warn(ex, $"Unexpected end of transmission");
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
					RaknetHandler.HandleOfflineRakMessage(receivedBytes, clientEndpoint);
				}
				else
				{
					Log.Warn($"Receive invalid message, but not a RakNet message. Message ID={messageId}. Ignoring.");
				}

				return;
			}

			var rakSession = Session;

			if (rakSession == null || !Equals(Session.EndPoint, clientEndpoint))
				return;

			/*if (!RakSessions.TryGetValue(clientEndpoint, out RaknetSession rakSession))
			{
				return;
			}*/

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
				rakSession.Disconnect("Bad packet received from server.");

				Log.Warn(e, $"Bad packet {receivedBytes.Span[0]}\n{Packet.HexDump(receivedBytes)}");
				return;
			}

			if (Log.IsTraceEnabled) Log.Trace($"Receive datagram #{datagram.Header.DatagramSequenceNumber} for {_endpoint}");

			EnqueueAck(rakSession, datagram.Header.DatagramSequenceNumber);
			
			HandleDatagram(rakSession, datagram);
			datagram.PutPool();
		}

		private void HandleDatagram(RaknetSession session, Datagram datagram)
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
				session.HandleRakMessage(datagram, message);
			}
		}

		private Packet HandleSplitMessage(RaknetSession session, SplitPartPacket splitPart)
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

			if (Log.IsTraceEnabled) Log.Trace($"Got all {spCount} split packets for split ID: {spId}");

			session.Splits.TryRemove(spId, out SplitPartPacket[] _);

			int contiguousLength = 0;
			foreach (SplitPartPacket spp in splitPartList)
			{
				contiguousLength += spp.Message.Length;
			}

			var buffer = new Memory<byte>(new byte[contiguousLength]);

			Reliability headerReliability           = splitPart.ReliabilityHeader.Reliability;
			var         headerReliableMessageNumber = splitPart.ReliabilityHeader.ReliableMessageNumber;
			var         headerOrderingChannel       = splitPart.ReliabilityHeader.OrderingChannel;
			var         headerOrderingIndex         = splitPart.ReliabilityHeader.OrderingIndex;
			var         headerSequencingIndex       = splitPart.ReliabilityHeader.SequencingIndex;

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
					SequencingIndex = headerSequencingIndex
				};

				if (Log.IsTraceEnabled) Log.Trace($"Assembled split packet {fullMessage.ReliabilityHeader.Reliability} message #{fullMessage.ReliabilityHeader.ReliableMessageNumber}, OrdIdx: #{fullMessage.ReliabilityHeader.OrderingIndex}");

				return fullMessage;
			}
			catch (Exception e)
			{
				Log.Error(e, "Error during split message parsing");
				if (Log.IsDebugEnabled) Log.Debug($"0x{buffer.Span[0]:x2}\n{Packet.HexDump(buffer)}");
				session.Disconnect("Bad packet received from client.", false);
			}

			return null;
		}


		private void EnqueueAck(RaknetSession session, Int24 datagramSequenceNumber)
		{
			session.Acknowledge(datagramSequenceNumber);
		}

		private void HandleAck(RaknetSession session, Ack ack, ConnectionInfo connectionInfo)
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

		internal void HandleNak(RaknetSession session, Nak nak, ConnectionInfo connectionInfo)
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
							Log.Warn($"NAK, no datagram #{i} for {session.Username}");
					}
				}
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CalculateRto(RaknetSession session, Datagram datagram)
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
			if (Session != null)
				await Session.SendTickAsync(this);
		}

		internal async Task UpdateAsync(RaknetSession session)
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
					Session.DetectLostConnection();
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

		public void SendPacket(RaknetSession session, Packet message)
		{
			foreach (Datagram datagram in Datagram.CreateDatagrams(message, session.MtuSize, session))
			{
				SendDatagram(session, datagram);
			}

			message.PutPool();
		}
		
		public async Task SendPacketAsync(RaknetSession session, Packet message)
		{
			foreach (Datagram datagram in Datagram.CreateDatagrams(message, session.MtuSize, session))
			{
				await SendDatagramAsync(session, datagram);
			}

			message.PutPool();
		}

		public async Task SendPacketAsync(RaknetSession session, List<Packet> messages)
		{
			await Task.WhenAll(
				Datagram.CreateDatagrams(messages, session.MtuSize, session)
				   .Select(async x => await SendDatagramAsync(session, x)));
			
			//foreach (Datagram datagram in Datagram.CreateDatagrams(messages, session.MtuSize, session))
			//{
			//	await SendDatagramAsync(session, datagram);
			//}

			foreach (Packet message in messages)
			{
				message.PutPool();
			}
		}

		
		public void SendDatagram(RaknetSession session, Datagram datagram)
		{
			SendDatagramAsync(session, datagram).Wait();

			return;
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

			//Memory<byte> buffer = new byte[1600];;// ArrayPool<byte>.Shared.Rent(1600);
			//int      length = (int) datagram.GetEncoded(ref buffer);

			datagram.Timer.Restart();

			if (!ConnectionInfo.DisableAck && !ConnectionInfo.IsEmulator && !session.WaitingForAckQueue.TryAdd(datagram.Header.DatagramSequenceNumber.IntValue(), datagram))
			{
				Log.Warn($"Datagram sequence unexpectedly existed in the ACK/NAK queue already {datagram.Header.DatagramSequenceNumber.IntValue()}");
				datagram.PutPool();
			}

			//lock (session.SyncRoot)
			{
				SendData(datagram.Encode(), session.EndPoint);
				//ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		public async Task SendDatagramAsync(RaknetSession session, Datagram datagram)
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
			SendDataAsync(data, targetEndPoint).Wait();

			return;
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