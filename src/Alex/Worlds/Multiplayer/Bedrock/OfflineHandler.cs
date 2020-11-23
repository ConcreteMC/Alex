using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public sealed class RakOfflineHandler
	{
		private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

		public const int UdpHeaderSize = 28;

		public static short MtuSize { get; set; } = 1500;
		public static short MaxMtuSize { get; } = 1500;
		
		private readonly RakConnection _connection;
		private readonly ConnectionInfo _connectionInfo;

		public long ClientGuid { get; }

		// RakNet found a remote server using Ping.
		public bool HaveServer { get; set; }

		// Tell RakNet to automatically connect to any found server.
		public bool AutoConnect { get; set; } = true;

		internal RakOfflineHandler(RakConnection connection, ConnectionInfo connectionInfo)
		{
			_connection = connection;
			_connectionInfo = connectionInfo;

			byte[] buffer = new byte[8];
			new Random().NextBytes(buffer);
			ClientGuid = BitConverter.ToInt64(buffer, 0);
		}

		internal void HandleOfflineRakMessage(ReadOnlyMemory<byte> receiveBytes, IPEndPoint senderEndpoint)
		{
			byte messageId = receiveBytes.Span[0];
			var messageType = (DefaultMessageIdTypes) messageId;

			// Increase fast, decrease slow on 1s ticks.
			if (_connectionInfo.NumberOfPlayers < _connectionInfo.RakSessions.Count) _connectionInfo.NumberOfPlayers = _connectionInfo.RakSessions.Count;

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
					Log.Error($"Receive bad packet with ID: {messageId} (0x{messageId:x2}) {messageType} from {senderEndpoint.Address}");

					return;
				}

				TraceReceive(Log, message);

				switch (messageType)
				{
					case DefaultMessageIdTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
						// Stop this client connection
						_connection.Stop();
						break;
					case DefaultMessageIdTypes.ID_UNCONNECTED_PING:
					case DefaultMessageIdTypes.ID_UNCONNECTED_PING_OPEN_CONNECTIONS:
						HandleRakNetMessage(senderEndpoint, (UnconnectedPing) message);
						break;
					case DefaultMessageIdTypes.ID_UNCONNECTED_PONG:
						HandleRakNetMessage(senderEndpoint, (UnconnectedPong) message);
						break;
					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REQUEST_1:
						//HandleRakNetMessage(senderEndpoint, (OpenConnectionRequest1) message);
						break;
					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_1:
						HandleRakNetMessage(senderEndpoint, (OpenConnectionReply1) message);
						break;
					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REQUEST_2:
						//HandleRakNetMessage(senderEndpoint, (OpenConnectionRequest2) message);
						break;
					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_2:
						HandleRakNetMessage(senderEndpoint, (OpenConnectionReply2) message);
						break;
					default:
						if (Log.IsInfoEnabled) Log.Error($"Receive unexpected packet with ID: {messageId} (0x{messageId:x2}) {messageType} from {senderEndpoint.Address}");
						break;
				}
			}
			finally
			{
				message?.PutPool();
			}
		}

		private void HandleRakNetMessage(IPEndPoint senderEndpoint, UnconnectedPing message)
		{

		}

		public void HandleRakNetMessage(IPEndPoint senderEndpoint, UnconnectedPong message)
		{
			Log.Warn($"Found server at {senderEndpoint}");
			Log.Warn($"MOTD: {message.serverName}");

			if (!HaveServer)
			{
				string[] motdParts = message.serverName.Split(';');
				if (motdParts.Length >= 11)
				{
					senderEndpoint.Port = int.Parse(motdParts[10]);
				}

				if (AutoConnect)
				{
					Log.Warn($"Connecting to {senderEndpoint}");
					HaveServer = true;
					SendOpenConnectionRequest1(senderEndpoint, MtuSize);
				}
				else
				{
					Log.Warn($"Connect to server using actual endpoint={senderEndpoint}");
					_connection.RemoteEndpoint = senderEndpoint;
					_connection.RemoteServerName = message.serverName;
					HaveServer = true;
				}
			}
		}

		public void SendOpenConnectionRequest1(IPEndPoint targetEndPoint, short mtuSize)
		{
			MtuSize = mtuSize; // This is what we will use from connections this point forward

			var packet = OpenConnectionRequest1.CreateObject();
			packet.raknetProtocolVersion = 10;
			packet.mtuSize = mtuSize;

			byte[] data = packet.Encode();

			TraceSend(packet);

			Log.Warn($"Sending MTU size={mtuSize}, data length={data.Length}");
			_connection.SendData(data, targetEndPoint);
		}

		private void HandleRakNetMessage(IPEndPoint senderEndpoint, OpenConnectionReply1 message)
		{
			if (message.mtuSize != MtuSize)
			{
				Log.Warn($"Error, mtu differ from what we sent. Received {message.mtuSize} bytes");
				//return;
			}

			Log.Warn($"Server with ID {message.serverGuid} security={message.serverHasSecurity}, mtu agreed on {message.mtuSize}");

			SendOpenConnectionRequest2(senderEndpoint, message.mtuSize);
		}

		private void SendOpenConnectionRequest2(IPEndPoint targetEndPoint, short mtuSize)
		{
			var packet = OpenConnectionRequest2.CreateObject();
			packet.remoteBindingAddress = targetEndPoint;
			packet.mtuSize = mtuSize;
			packet.clientGuid = ClientGuid;

			byte[] data = packet.Encode();

			TraceSend(packet);

			_connection.SendData(data, targetEndPoint);
		}
		
		private void HandleRakNetMessage(IPEndPoint senderEndpoint, OpenConnectionReply2 message)
		{
			if (HaveServer)
				return;
			Log.Warn("MTU Size: " + message.mtuSize);
			Log.Warn("Client Endpoint: " + message.clientEndpoint);

			HaveServer = true;

			SendConnectionRequest(senderEndpoint, message.mtuSize);
		}
		
		//public ConcurrentDictionary<IPEndPoint, RakSession> _sessions = new ConcurrentDictionary<IPEndPoint, RakSession>();
		private void SendConnectionRequest(IPEndPoint targetEndPoint, short mtuSize)
		{
			ConcurrentDictionary<IPEndPoint, RakSession> sessions = _connection.RakSessions;

			RakSession session;
			lock (sessions)
			{
				if (sessions.ContainsKey(targetEndPoint))
				{
					Log.Warn($"Session already exist, ignoring");
					return;
				}

				session = new RakSession(_connectionInfo, _connection, targetEndPoint, mtuSize)
				{
					State = ConnectionState.Connecting,
					LastUpdatedTime = DateTime.UtcNow,
					NetworkIdentifier = ClientGuid,
				};

				session.CustomMessageHandler = _connection.CustomMessageHandlerFactory?.Invoke(session);

				if (!sessions.TryAdd(targetEndPoint, session))
				{
					Log.Warn($"Session already exist, ignoring");
					return;
				}
			}

			var packet = ConnectionRequest.CreateObject();
			packet.clientGuid = ClientGuid;
			packet.timestamp = DateTime.UtcNow.Ticks;
			packet.doSecurity = 0;

			session.SendPacket(packet);
		}

		internal static void TraceReceive(ILogger log, Packet message)
		{
			if (!Log.IsTraceEnabled) return;

			try
			{
				string typeName = message.GetType().Name;

				string includePattern = Config.GetProperty("TracePackets.Include", ".*");
				string excludePattern = Config.GetProperty("TracePackets.Exclude", null);
				int verbosity = Config.GetProperty("TracePackets.Verbosity", 0);
				verbosity = Config.GetProperty($"TracePackets.Verbosity.{typeName}", verbosity);

				if (!Regex.IsMatch(typeName, includePattern))
				{
					return;
				}

				if (!string.IsNullOrWhiteSpace(excludePattern) && Regex.IsMatch(typeName, excludePattern))
				{
					return;
				}

				if (verbosity == 0)
				{
					log.Trace($"> Receive: {message.Id} (0x{message.Id:x2}): {message.GetType().Name}");
				}
				else if (verbosity == 1 || verbosity == 3)
				{
					var jsonSerializerSettings = new JsonSerializerSettings
					{
						PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
						TypeNameHandling = TypeNameHandling.Auto,
						Formatting = Formatting.Indented,
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

					jsonSerializerSettings.Converters.Add(new StringEnumConverter());
					jsonSerializerSettings.Converters.Add(new NbtIntConverter());
					jsonSerializerSettings.Converters.Add(new NbtStringConverter());
					jsonSerializerSettings.Converters.Add(new IPAddressConverter());
					jsonSerializerSettings.Converters.Add(new IPEndPointConverter());

					string result = JsonConvert.SerializeObject(message, jsonSerializerSettings);
					log.Trace($"> Receive: {message.Id} (0x{message.Id:x2}): {message.GetType().Name}\n{result}");
				}
				else if (verbosity == 2 || verbosity == 3)
				{
					log.Trace($"> Receive: {message.Id} (0x{message.Id:x2}): {message.GetType().Name}\n{Packet.HexDump(message.Bytes)}");
				}
			}
			catch (Exception e)
			{
				log.Error("Error when printing trace", e);
			}
		}

		internal static void TraceSend(Packet message)
		{
			if (!Log.IsTraceEnabled) return;

			try
			{
				string typeName = message.GetType().Name;

				string includePattern = Config.GetProperty("TracePackets.Include", ".*");
				string excludePattern = Config.GetProperty("TracePackets.Exclude", null);
				int verbosity = Config.GetProperty("TracePackets.Verbosity", 0);
				verbosity = Config.GetProperty($"TracePackets.Verbosity.{typeName}", verbosity);

				if (!Regex.IsMatch(typeName, includePattern))
				{
					return;
				}

				if (!string.IsNullOrWhiteSpace(excludePattern) && Regex.IsMatch(typeName, excludePattern))
				{
					return;
				}

				if (verbosity == 0)
				{
					Log.Trace($"<    Send: {message.Id} (0x{message.Id:x2}): {message.GetType().Name}");
				}
				else if (verbosity == 1 || verbosity == 3)
				{
					var jsonSerializerSettings = new JsonSerializerSettings
					{
						PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
						TypeNameHandling = TypeNameHandling.Auto,
						Formatting = Formatting.Indented,
						DefaultValueHandling = DefaultValueHandling.Include,
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

					jsonSerializerSettings.Converters.Add(new StringEnumConverter());
					jsonSerializerSettings.Converters.Add(new NbtIntConverter());
					jsonSerializerSettings.Converters.Add(new NbtStringConverter());
					jsonSerializerSettings.Converters.Add(new IPAddressConverter());
					jsonSerializerSettings.Converters.Add(new IPEndPointConverter());

					string result = JsonConvert.SerializeObject(message, jsonSerializerSettings);
					Log.Trace($"<    Send: {message.Id} (0x{message.Id:x2}): {message.GetType().Name}\n{result}");
				}
				else if (verbosity == 2 || verbosity == 3)
				{
					Log.Trace($"<    Send: {message.Id} (0x{message.Id:x2}): {message.GetType().Name}\n{Packet.HexDump(message.Bytes)}");
				}
			}
			catch (Exception e)
			{
				Log.Error("Error when printing trace", e);
			}
		}
	}
}