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
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using NLog;

namespace Alex.Networking.Bedrock.RakNet
{
	public class RaknetConnection
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		private UdpClient _listener;
		private readonly IPEndPoint _endpoint;

		public const int UdpHeaderSize = 28;

		public short MtuSize { get; set; } = 1400;

		public RaknetSession Session { get; set; } = null;
		public Interfaces.Net.ConnectionInfo ConnectionInfo { get; }

		public bool FoundServer => HaveServer;

		public bool IsNetworkOutOfOrder => Session?.IsOutOfOrder ?? false;

		// This is only used in client scenarios. Will contain
		// information regarding a located server.
		public IPEndPoint RemoteEndpoint { get; set; }
		public string RemoteServerName { get; set; }
		public long RemoteServerPing { get; set; } = 0;

		public Func<RaknetSession, ICustomMessageHandler> CustomMessageHandlerFactory { get; set; }

		// RakNet found a remote server using Ping.
		public bool HaveServer { get; set; }

		// Tell RakNet to automatically connect to any found server.
		public bool AutoConnect { get; set; } = true;

		public long ClientGuid { get; }
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		
		public RaknetConnection()
		{
			_endpoint = new IPEndPoint(IPAddress.Any, 0);

			ConnectionInfo = new Interfaces.Net.ConnectionInfo();

			byte[] buffer = new byte[8];
			new Random().NextBytes(buffer);
			ClientGuid = BitConverter.ToInt64(buffer, 0);
		}

		public void Start()
		{
			if (_listener != null) return;
			_listener = CreateListener(_endpoint);
			_listener.BeginReceive(ReceiveCallback, _listener);
		}

		public bool TryConnect(IPEndPoint targetEndPoint,
			int numberOfAttempts = int.MaxValue,
			short mtuSize = 1400,
			CancellationToken cancellationToken = default)
		{
			if (targetEndPoint.AddressFamily != AddressFamily.InterNetwork)
				return false;

			try
			{
				Start(); // Make sure we have started the listener

				SpinWait sw = new SpinWait();

				while (Session == null && numberOfAttempts > 0 && mtuSize >= UdpHeaderSize
				       && !cancellationToken.IsCancellationRequested)
				{
					if (!HaveServer)
					{
						if (!ConnectionResetEvent.Wait(500, cancellationToken))
						{
							if (numberOfAttempts % 4 == 0)
							{
								mtuSize -= UdpHeaderSize;
								//	Log.Info($"Adjusted mtu size: {mtuSize}");
							}

							SendOpenConnectionRequest1(targetEndPoint, mtuSize);
							numberOfAttempts--;
						}
					}

					sw.SpinOnce();
				}

				if (Session == null) return false;
			}
			catch (OperationCanceledException)
			{
				return false;
			}

			return true;
		}

		public void SendUnconnectedPingInternal(IPEndPoint targetEndPoint)
		{
			byte[] data = new UnconnectedPing() { pingId = DateTime.UtcNow.ToBinary(), guid = ClientGuid }.Encode();

			if (targetEndPoint != null)
				SendData(data, targetEndPoint);
			else
				SendData(data, new IPEndPoint(IPAddress.Broadcast, 19132));
		}

		private bool _stopped = false;

		public void Stop()
		{
			try
			{
				if (_stopped)
					return;

				_stopped = true;

				//Log.Info("Shutting down...");
				Session?.Close();

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

			if (Environment.OSVersion.Platform != PlatformID.MacOSX && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				//listener.Client.ReceiveBufferSize = 1600*64;
				listener.Client.ReceiveBufferSize = int.MaxValue;
				//listener.Ttl = Int16.MaxValue;
				//listener.Client.SendBufferSize = 1600*64;
				listener.Client.SendBufferSize = int.MaxValue;

				listener.DontFragment = true;
				listener.EnableBroadcast = false;
			}

			listener.Client.Blocking = true;
			listener.Client.Bind(endpoint);

			return listener;
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			var thread = Thread.CurrentThread;

			if (thread == null)
				return;
			
			thread.Name = $"RaknetConnection Read ({_endpoint})";

			bool hasReadData = false;

			//UdpClient listener;
			//while (_listener != null)
			{
				var listener = _listener;

				if (listener == null)
					return;

				// Check if we already closed the server
				//	if (listener?.Client == null) return;

				// WSAECONNRESET:
				// The virtual circuit was reset by the remote side executing a hard or abortive close. 
				// The application should close the socket; it is no longer usable. On a UDP-datagram socket 
				// this error indicates a previous send operation resulted in an ICMP Port Unreachable message.
				// Note the spocket settings on creation of the server. It makes us ignore these resets.
				IPEndPoint senderEndpoint = null;

				try
				{
					var receiveBytes = listener.EndReceive(ar, ref senderEndpoint);
					listener.BeginReceive(ReceiveCallback, listener);
					//var receive = await listener.ReceiveAsync();;
					//var receiveBytes = listener.Receive(ref senderEndpoint);

					//	var receiveBytes = receive.Buffer;
					//senderEndpoint = receive.RemoteEndPoint;

					//Interlocked.Increment(ref ConnectionInfo.PacketsIn);

					if (receiveBytes.Length != 0)
					{
						hasReadData = true;
						Interlocked.Add(ref ConnectionInfo.BytesIn, receiveBytes.Length);

						try
						{
							ReceiveDatagram(receiveBytes, senderEndpoint);
						}
						catch (Exception e)
						{
							Log.Warn(e, $"Process message error from: {senderEndpoint.Address}");
						}
					}
					else
					{
						Log.Warn(
							$"Unexpected end of transmission for {RemoteEndpoint} from {senderEndpoint} (Any data received? {hasReadData} Servername: {RemoteServerName})");
					}
				}
				catch (ObjectDisposedException) { }
				catch (SocketException e)
				{
					// 10058 (just regular disconnect while listening)
					if (e.ErrorCode == 10058) return;
					if (e.ErrorCode == 10038) return;
					if (e.ErrorCode == 10004) return;
					if (e.ErrorCode == 4) return;

					if (Log.IsDebugEnabled) Log.Error(e, "Unexpected end of receive");
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

				if (messageId <= (byte)DefaultMessageIdTypes.ID_USER_PACKET_ENUM)
				{
					HandleOfflineRakMessage(receivedBytes, clientEndpoint);
				}
				else
				{
					Log.Warn($"Receive invalid message, but not a RakNet message. Message ID={messageId}. Ignoring.");
				}

				return;
			}

			var rakSession = Session;

			if (rakSession == null)
				return;

			if (rakSession.Evicted) return;

			rakSession.LastUpdatedTime = DateTime.UtcNow;

			if (header.IsAck)
			{
				var ack = Ack.CreateObject();
				ack.Decode(receivedBytes);

				rakSession.HandleAck(ack);

				ack.PutPool();

				return;
			}

			if (header.IsNak)
			{
				var nak = Nak.CreateObject();
				nak.Decode(receivedBytes);

				rakSession.HandleNak(nak);
				nak.PutPool();

				return;
			}

			var datagram = Datagram.CreateObject();

			try
			{
				try
				{
					datagram.Decode(receivedBytes);

					Session.Acknowledge(datagram);

					{
						if (datagram.Header.IsPacketPair)
						{
							Session.CongestionManager.OnGotPacketPair(
								datagram.Header.DatagramSequenceNumber.IntValue());
						}

						Interlocked.Increment(ref ConnectionInfo.PacketsIn);

						//if (Log.IsTraceEnabled) Log.Trace($"Receive datagram #{datagram.Header.DatagramSequenceNumber} for {_endpoint}");
						foreach (var packet in datagram.Messages)
						{
							var message = packet;

							if (message is SplitPartPacket splitPartPacket)
							{
								message = HandleSplitMessage(splitPartPacket);
							}

							if (message == null) continue;

							message.Timer.Restart();
							Session.HandleRakMessage(message);
						}
					}
				}
				catch (Exception e)
				{
					rakSession.Disconnect("Bad packet received from server.");

					Log.Warn(
						e,
						$"Bad packet: {receivedBytes.Span[0]}\n{Packet.HexDump(receivedBytes)} ({datagram.Header.ToString()})");

					return;
				}
			}
			finally
			{
				datagram?.PutPool();
			}
		}

		private Packet HandleSplitMessage(SplitPartPacket splitPart)
		{
			int spId = splitPart.ReliabilityHeader.PartId;
			int spIdx = splitPart.ReliabilityHeader.PartIndex;
			int spCount = splitPart.ReliabilityHeader.PartCount;

			SplitPartPacket[] splitPartList = Session.Splits.GetOrAdd(spId, new SplitPartPacket[spCount]);
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

			//if (Log.IsTraceEnabled)Log.Trace($"Got all {spCount} split packets for split ID: {spId}");

			Session.Splits.TryRemove(spId, out SplitPartPacket[] _);

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
			var headerSequencingIndex = splitPart.ReliabilityHeader.SequencingIndex;

			int position = 0;

			foreach (SplitPartPacket spp in splitPartList)
			{
				spp.Message.CopyTo(buffer.Slice(position));
				position += spp.Message.Length;
				spp.PutPool();
			}

			try
			{
				Packet fullMessage = PacketFactory.Create(buffer.Span[0], buffer, "raknet")
				                     ?? new UnknownPacket(buffer.Span[0], buffer.ToArray());

				fullMessage.ReliabilityHeader = new ReliabilityHeader()
				{
					Reliability = headerReliability,
					ReliableMessageNumber = headerReliableMessageNumber,
					OrderingChannel = headerOrderingChannel,
					OrderingIndex = headerOrderingIndex,
					SequencingIndex = headerSequencingIndex
				};

				//	if (Log.IsTraceEnabled) Log.Trace($"Assembled split packet {fullMessage.ReliabilityHeader.Reliability} message #{fullMessage.ReliabilityHeader.ReliableMessageNumber}, OrdIdx: #{fullMessage.ReliabilityHeader.OrderingIndex}");

				return fullMessage;
			}
			catch (Exception e)
			{
				Log.Error(e, "Error during split message parsing");
				if (Log.IsDebugEnabled) Log.Debug($"0x{buffer.Span[0]:x2}\n{Packet.HexDump(buffer)}");
				Session.Disconnect("Bad packet received from client.", false);
			}

			return null;
		}

		public int SendDatagram(RaknetSession session, Datagram datagram)
		{
			if (datagram.MessageParts.Count == 0)
			{
				Log.Warn($"Failed to send #{datagram.Header.DatagramSequenceNumber.IntValue()}");
				datagram.PutPool();

				return 0;
			}

			if (datagram.TransmissionCount > 10)
			{
				if (Log.IsDebugEnabled)
					Log.Warn(
						$"Retransmission count exceeded. No more resend of #{datagram.Header.DatagramSequenceNumber.IntValue()} Type: {datagram.FirstMessageId} (0x{datagram.FirstMessageId:x2})");

				datagram.PutPool();

				Interlocked.Increment(ref ConnectionInfo.Fails);

				//TODO: Disconnect! Because of encryption, this connection can't be used after this point
				return 0;
			}

			var sequenceNumber =
				(int)session.CongestionManager
				   .GetAndIncrementNextDatagramSequenceNumber(); // Interlocked.Increment(ref session.DatagramSequenceNumber);

			long rto = session.CongestionManager.GetRtoForRetransmission();
			datagram.TransmissionCount++;
			datagram.RetransmissionTimeOut = rto;
			datagram.Header.DatagramSequenceNumber = sequenceNumber;
			datagram.RetransmitImmediate = false;
			datagram.Header.HasBAndAs = false;
			//datagram.Header.NeedsBAndAs = session.SlidingWindow.IsInSlowStart();
			//datagram.Header.IsContinuousSend = session.SlidingWindow.IsContinuousSend;
			//datagram.Header.IsContinuousSend = session.SlidingWindow.

			byte[] buffer = null;

			try
			{
				if (!session.WaitingForAckQueue.TryAdd(sequenceNumber, datagram))
				{
					Log.Warn(
						$"Datagram sequence unexpectedly existed in the ACK/NAK queue already {datagram.Header.DatagramSequenceNumber.IntValue()}");

					datagram.PutPool();

					return 0;
				}

				buffer = ArrayPool<byte>.Shared.Rent(1600);

				int length = (int)datagram.GetEncoded(ref buffer);
				//byte[] buffer = datagram.Encode();
				//int length = buffer.Length;
				//session.UnackedBytes += datagram.Size;

				Interlocked.Increment(ref ConnectionInfo.PacketsOut);
				SendData(buffer, length, session.EndPoint);

				datagram.Timer.Restart();

				return length;
			}
			finally
			{
				if (buffer != null)
					ArrayPool<byte>.Shared.Return(buffer);
			}
		}


		public void SendData(byte[] data, IPEndPoint targetEndPoint)
		{
			SendData(data, data.Length, targetEndPoint);
		}

		private object _sendSync = new object();

		private void SendData(byte[] data, int length, IPEndPoint targetEndPoint)
		{
			if (length <= 0)
				return;

			//Monitor.Enter(_sendSync);

			try
			{
				try
				{
					if (_listener == null)
					{
						return;
					}

					int sent = _listener.Send(data, length, targetEndPoint);

					if (sent != length)
					{
						throw new Exception($"Wrote {sent} out of {length} bytes");
					}
					//_listener.Send(data, length, targetEndPoint);

					//Interlocked.Increment(ref ConnectionInfo.PacketsOut);
					Interlocked.Add(ref ConnectionInfo.BytesOut, length);
				}
				catch (ObjectDisposedException e)
				{
					Log.Warn(e);
				}
				catch (Exception e)
				{
					Log.Warn(
						e,
						$"Error. Data length={data.Length} Length={length} Target={targetEndPoint} Family: {targetEndPoint.AddressFamily.ToString()}");
					//if(_listener == null || _listener.Client != null) Log.Error(string.Format("Send data lenght: {0}", data.Length), e);
				}
			}
			finally
			{
				//	Monitor.Exit(_sendSync);
			}
		}


		internal void HandleOfflineRakMessage(ReadOnlyMemory<byte> receiveBytes, IPEndPoint senderEndpoint)
		{
			byte messageId = receiveBytes.Span[0];
			var messageType = (DefaultMessageIdTypes)messageId;

			// Increase fast, decrease slow on 1s ticks.
			//if (_connectionInfo.NumberOfPlayers < _connectionInfo.RakSessions.Count) _connectionInfo.NumberOfPlayers = _connectionInfo.RakSessions.Count;

			Packet message = null;

			try
			{
				try
				{
					message = PacketFactory.Create(messageId, receiveBytes, "raknet");
				}
				catch (Exception)
				{
					message = null;
				}

				if (message == null)
				{
					Log.Error(
						$"Receive bad packet with ID: {messageId} (0x{messageId:x2}) {messageType} from {senderEndpoint.Address}");

					return;
				}

				//	TraceReceive(Log, message);

				switch (messageType)
				{
					case DefaultMessageIdTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
						// Stop this client connection
						Stop();

						break;

					case DefaultMessageIdTypes.ID_UNCONNECTED_PING:
					case DefaultMessageIdTypes.ID_UNCONNECTED_PING_OPEN_CONNECTIONS:
						//HandleRakNetMessage(senderEndpoint, (UnconnectedPing) message);
						break;

					case DefaultMessageIdTypes.ID_UNCONNECTED_PONG:
						HandleRakNetMessage(senderEndpoint, (UnconnectedPong)message);

						break;

					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REQUEST_1:
						//HandleRakNetMessage(senderEndpoint, (OpenConnectionRequest1) message);
						break;

					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_1:
						HandleRakNetMessage(senderEndpoint, (OpenConnectionReply1)message);

						break;

					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REQUEST_2:
						//HandleRakNetMessage(senderEndpoint, (OpenConnectionRequest2) message);
						break;

					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_2:
						HandleRakNetMessage(senderEndpoint, (OpenConnectionReply2)message);

						break;

					default:
						if (Log.IsInfoEnabled)
							Log.Error(
								$"Receive unexpected packet with ID: {messageId} (0x{messageId:x2}) {messageType} from {senderEndpoint.Address}");

						break;
				}
			}
			finally
			{
				message?.PutPool();
			}
		}

		private void HandleRakNetMessage(IPEndPoint senderEndpoint, UnconnectedPong message)
		{
			//Log.Warn($"Found server at {senderEndpoint}");
			//Log.Warn($"MOTD: {message.serverName}");

			if (!HaveServer)
			{
				string[] motdParts = message.serverName.Split(';');

				if (motdParts.Length >= 11)
				{
					senderEndpoint.Port = int.Parse(motdParts[10]);
				}

				var currentTime = DateTime.UtcNow;
				var pingId = DateTime.FromBinary(message.pingId);
				var elapsedTime = currentTime - pingId;

				RemoteEndpoint = senderEndpoint;
				RemoteServerName = message.serverName;
				RemoteServerPing = (long)Math.Round(elapsedTime.TotalMilliseconds);

				HaveServer = true;
			}
		}

		public ManualResetEventSlim ConnectionResetEvent = new ManualResetEventSlim(false);

		public void SendOpenConnectionRequest1(IPEndPoint targetEndPoint, short mtuSize)
		{
			//if (ConnectionResetEvent.IsSet) return;

			var packet = OpenConnectionRequest1.CreateObject();
			packet.raknetProtocolVersion = 10;
			packet.mtuSize = (short)(mtuSize);

			byte[] data = packet.Encode();

			MtuSize = mtuSize; // (short) (mtuSize + (data.Length - mtuSize)); // This is what we will use from connections this point forward

			//TraceSend(packet);

			SendData(data, targetEndPoint);

			packet.PutPool();
		}

		private void HandleRakNetMessage(IPEndPoint senderEndpoint, OpenConnectionReply1 message)
		{
			//if (HaveServer)
			//	return;

			if (message.mtuSize != MtuSize)
			{
				Log.Warn($"Error, mtu differ from what we sent. Received {message.mtuSize}, expected {MtuSize}");

				if (message.mtuSize > MtuSize + 10)
					return;
			}

			ConnectionResetEvent.Set();

			SendOpenConnectionRequest2(senderEndpoint, message.mtuSize);
		}

		public void SendOpenConnectionRequest2(IPEndPoint targetEndPoint, short mtuSize)
		{
			var packet = OpenConnectionRequest2.CreateObject();
			packet.remoteBindingAddress = targetEndPoint;
			packet.mtuSize = mtuSize;
			packet.clientGuid = ClientGuid;

			byte[] data = packet.Encode();

			SendData(data, targetEndPoint);

			packet.PutPool();
		}

		private void HandleRakNetMessage(IPEndPoint senderEndpoint, OpenConnectionReply2 message)
		{
			HaveServer = true;

			SendConnectionRequest(senderEndpoint, message.mtuSize);
		}

		//public ConcurrentDictionary<IPEndPoint, RakSession> _sessions = new ConcurrentDictionary<IPEndPoint, RakSession>();
		private void SendConnectionRequest(IPEndPoint targetEndPoint, short mtuSize)
		{
			RaknetSession session = Session;

			//	lock (session)
			{
				if (session != null)
				{
					Log.Warn($"Session already exist, ignoring");

					return;
				}

				MtuSize = mtuSize;
				Log.Warn($"MTU Size: {mtuSize} Endpoint: {targetEndPoint}");

				session = new RaknetSession(ConnectionInfo, this, targetEndPoint, mtuSize)
				{
					State = ConnectionState.Connecting,
					LastUpdatedTime = DateTime.UtcNow,
					NetworkIdentifier = ClientGuid,
				};

				session.CustomMessageHandler = CustomMessageHandlerFactory?.Invoke(session);

				Session = session;
				/*if (!sessions.TryAdd(targetEndPoint, session))
				{
					Log.Warn($"Session already exist, ignoring");
					return;
				}*/
			}

			var packet = ConnectionRequest.CreateObject();
			packet.clientGuid = ClientGuid;
			packet.timestamp = DateTime.UtcNow.Ticks;
			packet.doSecurity = 0;

			session.SendPacket(packet);
		}
	}
}