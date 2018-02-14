using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using fNbt;
using Jose;
using log4net;
using MiNET;
using MiNET.BlockEntities;
using MiNET.Items;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;

namespace Alex.Network
{
	public class MiNetClient
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MiNetClient));

		private IPEndPoint _clientEndpoint;
		private readonly IPEndPoint _serverEndpoint;
		private short _mtuSize = 1192;
		private int ClientId { get; }

		private PlayerNetworkSession Session { get; set; }

		private int _clientGuid;
		public bool HaveServer = false;
		private UdpClient UdpClient { get; set; }

		private string Username { get; set; }
		private Thread _mainProcessingThread;
		public MiNetClient(IPEndPoint endpoint, string username)
		{
			Username = username;
			ClientId = new Random().Next();
			_serverEndpoint = endpoint;
		}


		/// <summary>
		/// Connects to the server.
		/// </summary>
		/// <param name="force">If true, forces the client to re-connect if already connected.</param>
		public bool Connect(bool force = false)
		{
			if (!force && UdpClient != null) return false; //Already initialized?
			UdpClient?.Close();

			try
			{
				_clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
				UdpClient = new UdpClient(_clientEndpoint)
				{
					Client =
					{
						ReceiveBufferSize = int.MaxValue,
						SendBufferSize = int.MaxValue
					},
					DontFragment = false
				};

				uint IOC_IN = 0x80000000;
				uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				UdpClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

				Session = new PlayerNetworkSession(null, null, _clientEndpoint, _mtuSize);

				//UdpClient.BeginReceive(ReceiveCallback, UdpClient);
				_mainProcessingThread = new Thread(ProcessDatagrams) { IsBackground = true };
				_mainProcessingThread.Start(UdpClient);

				_clientEndpoint = (IPEndPoint)UdpClient.Client.LocalEndPoint;

				SendUnconnectedPing();

				SendOpenConnectionRequest1();

				//Task.Run(ProcessQueue);

				Log.InfoFormat("{0} connected to {1}", _clientEndpoint.ToString(), _serverEndpoint.ToString());
				HaveServer = true;

				return true;
			}
			catch (Exception e)
			{
				Log.Error("Main loop", e);
				Disconnect();
			}
			return false;
		}

		/// <summary>
		///     Stops the client.
		/// </summary>
		/// <returns></returns>
		public bool Disconnect()
		{
			try
			{
				if (UdpClient == null) return true;

				UdpClient.Close();
				UdpClient = null;

				if (_mainProcessingThread != null)
				{
					if (_mainProcessingThread.IsAlive)
					{
						_mainProcessingThread.Abort();
					}

					_mainProcessingThread = null;
				}

				Log.InfoFormat("Connection closed {0}", _clientEndpoint.ToString());

				return true;
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			return false;
		}

		private void ProcessDatagrams(object state)
		{
			while (true)
			{
				UdpClient listener = (UdpClient)state;


				// Check if we already closed the server
				if (listener.Client == null) return;

				// WSAECONNRESET:
				// The virtual circuit was reset by the remote side executing a hard or abortive close. 
				// The application should close the socket; it is no longer usable. On a UDP-datagram socket 
				// this error indicates a previous send operation resulted in an ICMP Port Unreachable message.
				// Note the spocket settings on creation of the server. It makes us ignore these resets.
				IPEndPoint senderEndpoint = null;
				Byte[] receiveBytes = null;
				try
				{
					//var result = listener.ReceiveAsync().Result;
					//senderEndpoint = result.RemoteEndPoint;
					//receiveBytes = result.Buffer;
					receiveBytes = listener.Receive(ref senderEndpoint);

					if (receiveBytes.Length != 0)
					{
						ThreadPool.QueueUserWorkItem(obj =>
						{
							try
							{
								ProcessMessage(receiveBytes, senderEndpoint);
							}
							catch (Exception e)
							{
								Log.Warn(string.Format("Process message error from: {0}", senderEndpoint.Address), e);
							}
						});
					}
					else
					{
						Log.Error("Unexpected end of transmission?");
						return;
					}
				}
				catch (Exception e)
				{
					Log.Error("Unexpected end of transmission?", e);
					if (listener.Client != null)
					{
						continue;
					}

					return;
				}
			}
		}

		/// <summary>
		///     Handles the callback.
		/// </summary>
		/// <param name="ar">The results</param>
		private void ReceiveCallback(IAsyncResult ar)
		{
		//	Logging.Info("ReceiveCallBack Called");
			UdpClient listener = (UdpClient)ar.AsyncState;
			
			if (listener.Client == null) return;

			// WSAECONNRESET:
			// The virtual circuit was reset by the remote side executing a hard or abortive close. 
			// The application should close the socket; it is no longer usable. On a UDP-datagram socket 
			// this error indicates a previous send operation resulted in an ICMP Port Unreachable message.
			// Note the spocket settings on creation of the server. It makes us ignore these resets.
			IPEndPoint senderEndpoint = new IPEndPoint(0, 0);
			Byte[] receiveBytes;
			try
			{
				receiveBytes = listener.EndReceive(ar, ref senderEndpoint);
			}
			catch (Exception e)
			{
				if (listener.Client == null) return;
				Log.Error("Recieve processing: " + e);

				try
				{
					listener.BeginReceive(ReceiveCallback, listener);
				}
				catch (ObjectDisposedException dex)
				{
					// Log and move on. Should probably free up the player and remove them here.
					Log.Error("Recieve processing: " + dex);
				}

				return;
			}

			if (receiveBytes.Length != 0)
			{
				if (listener.Client == null) return;
				try
				{
					listener.BeginReceive(ReceiveCallback, listener);

					if (listener.Client == null) return;
					ProcessMessage(receiveBytes, senderEndpoint);
				}
				catch (Exception e)
				{
					Log.Error("Processing: " + e);
				}
			}
			else
			{
				Log.Error("Unexpected end of transmission?");
			}
		}

		/// <summary>
		///     Processes a message.
		/// </summary>
		/// <param name="receiveBytes">The received bytes.</param>
		/// <param name="senderEndpoint">The sender's endpoint.</param>
		/// <exception cref="System.Exception">Receive ERROR, NAK in wrong place</exception>
		private void ProcessMessage(byte[] receiveBytes, IPEndPoint senderEndpoint)
		{
			byte msgId = receiveBytes[0];

			if (msgId <= (byte)DefaultMessageIdTypes.ID_USER_PACKET_ENUM)
			{
				DefaultMessageIdTypes msgIdType = (DefaultMessageIdTypes)msgId;

				Package message = PackageFactory.CreatePackage(msgId, receiveBytes, "raknet");

				if (message == null) return;

			   // TraceReceive(message);

				switch (msgIdType)
				{
					case DefaultMessageIdTypes.ID_UNCONNECTED_PONG:
						{
							UnconnectedPong incoming = (UnconnectedPong)message;
							Log.Warn($"MOTD {incoming.serverName}");
							if (!HaveServer)
							{
							   // _serverEndpoint = senderEndpoint;
								HaveServer = true;
								SendOpenConnectionRequest1();
							}

							break;
						}
					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_1:
						{
							OpenConnectionReply1 incoming = (OpenConnectionReply1)message;
							if (incoming.mtuSize != _mtuSize) Log.Warn("Error:" + incoming.mtuSize);
							Log.Warn("Server Has Security" + incoming.serverHasSecurity);
							_mtuSize = incoming.mtuSize;
							SendOpenConnectionRequest2();
							break;
						}
					case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_2:
						{
							OnOpenConnectionReply2((OpenConnectionReply2)message);
							break;
						}
					case DefaultMessageIdTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
						{
							//OnNoFreeIncomingConnections((NoFreeIncomingConnections)message);
							break;
						}
				}
			}
			else
			{
				DatagramHeader header = new DatagramHeader(receiveBytes[0]);
				if (!header.isACK && !header.isNAK && header.isValid)
				{
					if (receiveBytes[0] == 0xa0)
					{
						throw new Exception("Receive ERROR, NAK in wrong place");
					}

					if (PlayerStatus == 3)
					{
						int datagramId = new Int24(new[] { receiveBytes[1], receiveBytes[2], receiveBytes[3] });

						//Acks ack = Acks.CreateObject();
						Acks ack = Acks.CreateObject();
						ack.acks.Add(datagramId);
						byte[] data = ack.Encode();
						ack.PutPool();
						SendData(data, senderEndpoint);

				   //     return;
					}

					ConnectedPackage package = ConnectedPackage.CreateObject();
					package.Decode(receiveBytes);
					header = package._datagramHeader;
					//Log.Info($"> Datagram #{header.datagramSequenceNumber}, {package._hasSplit}, {package._splitPacketId}, {package._reliability}, {package._reliableMessageNumber}, {package._sequencingIndex}, {package._orderingChannel}, {package._orderingIndex}");

					{
						Acks ack = Acks.CreateObject();
						ack.acks.Add(package._datagramSequenceNumber.IntValue());
						byte[] data = ack.Encode();
						ack.PutPool();
						SendData(data, senderEndpoint);
					}

					HandleConnectedPackage(package);
					package.PutPool();
				}
				else if (header.isPacketPair)
				{
					Log.Warn("header.isPacketPair");
				}
				else if (header.isACK && header.isValid)
				{
					HandleAck(receiveBytes, senderEndpoint);
				}
				else if (header.isNAK && header.isValid)
				{
					Nak nak = new Nak();
					nak.Decode(receiveBytes);
					HandleNak(receiveBytes, senderEndpoint);
				}
				else if (!header.isValid)
				{
					Log.Warn("!!!! ERROR, Invalid header !!!!!");
				}
				else
				{
					Log.Warn("!! WHAT THE F");
				}
			}
		}
		public int PlayerStatus { get; set; }

		private void HandleOpenConnectionReply2(OpenConnectionReply2 message)
		{
			SendConnectionRequest();
		}
		private void HandleAck(byte[] receiveBytes, IPEndPoint senderEndpoint)
		{

		}

		private void HandleNak(byte[] receiveBytes, IPEndPoint senderEndpoint)
		{

		}

		private void HandleConnectedPackage(ConnectedPackage package)
		{
			foreach (var message in package.Messages)
			{
				if (message is SplitPartPackage)
				{
					HandleSplitMessage(Session, (SplitPartPackage)message);
					continue;
				}

				//TraceReceive(message);

				message.Timer.Restart();
				AddToProcessing(message);
			}
		}


		private long _lastSequenceNumber = -1; // That's the first message with wrapper
		private AutoResetEvent _waitEvent = new AutoResetEvent(false);
		private AutoResetEvent _mainWaitEvent = new AutoResetEvent(false);
		private object _eventSync = new object();
		private ConcurrentPriorityQueue<int, Package> _queue = new ConcurrentPriorityQueue<int, Package>();

		private Thread _processingThread = null;

		public void AddToProcessing(Package message)
		{
			if (Session.CryptoContext == null || Session.CryptoContext.UseEncryption == false || message.Reliability != Reliability.ReliableOrdered)
			{
				HandlePackage(message);
				return;
			}

			//Log.Error("DO NOT USE THIS");
			//throw new Exception("DO NOT USE THIS");

			lock (_eventSync)
			{
				if (_lastSequenceNumber < 0) _lastSequenceNumber = 1;

				if (_queue.Count == 0 && message.OrderingIndex == _lastSequenceNumber + 1)
				{
					_lastSequenceNumber = message.OrderingIndex;
					HandlePackage(message);
					return;
				}

				if (_processingThread == null)
				{
					_processingThread = new Thread(ProcessQueueThread);
					_processingThread.IsBackground = true;
					_processingThread.Start();
				}

				_queue.Enqueue(message.OrderingIndex, message);
				WaitHandle.SignalAndWait(_waitEvent, _mainWaitEvent);
			}
		}

		private void ProcessQueueThread(object o)
		{
			ProcessQueue();
		}

		private Task ProcessQueue()
		{
			while (true)
			{
				KeyValuePair<int, Package> pair;

				if (_queue.TryPeek(out pair))
				{
					if (pair.Key == _lastSequenceNumber + 1)
					{
						if (_queue.TryDequeue(out pair))
						{
							_lastSequenceNumber = pair.Key;

							HandlePackage(pair.Value);

							if (_queue.Count == 0)
							{
								WaitHandle.SignalAndWait(_mainWaitEvent, _waitEvent, TimeSpan.FromMilliseconds(50), true);
							}
						}
					}
					else if (pair.Key <= _lastSequenceNumber)
					{
						if (Log.IsDebugEnabled) Log.Warn($"{Username} - Resent. Expected {_lastSequenceNumber + 1}, but was {pair.Key}.");
						if (_queue.TryDequeue(out pair))
						{
							pair.Value.PutPool();
						}
					}
					else
					{
						if (Log.IsDebugEnabled) Log.Warn($"{Username} - Wrong sequence. Expected {_lastSequenceNumber + 1}, but was {pair.Key}.");
						WaitHandle.SignalAndWait(_mainWaitEvent, _waitEvent, TimeSpan.FromMilliseconds(50), true);
					}
				}
				else
				{
					if (_queue.Count == 0)
					{
						WaitHandle.SignalAndWait(_mainWaitEvent, _waitEvent, TimeSpan.FromMilliseconds(50), true);
					}
				}
			}

			//Log.Warn($"Exit receive handler task for {Player.Username}");
			return Task.CompletedTask;
		}

		private void OnOpenConnectionReply2(OpenConnectionReply2 message)
		{
			Log.Warn("MTU Size: " + message.mtuSize);
			Log.Warn("Client Endpoint: " + message.clientEndpoint);

			//_serverEndpoint = message.clientEndpoint;

			_mtuSize = message.mtuSize;
			Thread.Sleep(100);
			SendConnectionRequest();
		}

		private void HandleSplitMessage(PlayerNetworkSession playerSession, SplitPartPackage splitMessage)
		{
			int spId = splitMessage.SplitId;
			int spIdx = splitMessage.SplitIdx;
			int spCount = splitMessage.SplitCount;

			Int24 sequenceNumber = splitMessage.DatagramSequenceNumber;
			Reliability reliability = splitMessage.Reliability;
			Int24 reliableMessageNumber = splitMessage.ReliableMessageNumber;
			Int24 orderingIndex = splitMessage.OrderingIndex;
			byte orderingChannel = splitMessage.OrderingChannel;

			if (!playerSession.Splits.ContainsKey(spId))
			{
				playerSession.Splits.TryAdd(spId, new SplitPartPackage[spCount]);
			}

			SplitPartPackage[] spPackets = playerSession.Splits[spId];
			spPackets[spIdx] = splitMessage;

			bool haveEmpty = false;
			for (int i = 0; i < spPackets.Length; i++)
			{
				haveEmpty = haveEmpty || spPackets[i] == null;
			}

			if (!haveEmpty)
			{
				Log.InfoFormat("Got all {0} split packages for split ID: {1}", spCount, spId);

				SplitPartPackage[] waste;
				playerSession.Splits.TryRemove(spId, out waste);

				MemoryStream stream = new MemoryStream();
				for (int i = 0; i < spPackets.Length; i++)
				{
					SplitPartPackage splitPartPackage = spPackets[i];
					byte[] buf = splitPartPackage.Message;
					if (buf == null)
					{
						Log.Error("Expected bytes in splitpart, but got none");
						continue;
					}

					stream.Write(buf, 0, buf.Length);
					splitPartPackage.PutPool();
				}

				byte[] buffer = stream.ToArray();
				try
				{
					ConnectedPackage newPackage = ConnectedPackage.CreateObject();
					newPackage._datagramSequenceNumber = sequenceNumber;
					newPackage._reliability = reliability;
					newPackage._reliableMessageNumber = reliableMessageNumber;
					newPackage._orderingIndex = orderingIndex;
					newPackage._orderingChannel = (byte)orderingChannel;
					newPackage._hasSplit = false;

					Package fullMessage = PackageFactory.CreatePackage(buffer[0], buffer, "raknet") ?? new UnknownPackage(buffer[0], buffer);
					fullMessage.DatagramSequenceNumber = sequenceNumber;
					fullMessage.Reliability = reliability;
					fullMessage.ReliableMessageNumber = reliableMessageNumber;
					fullMessage.OrderingIndex = orderingIndex;
					fullMessage.OrderingChannel = orderingChannel;

					newPackage.Messages = new List<Package>();
					newPackage.Messages.Add(fullMessage);

					Log.Info($"Assembled split package {newPackage._reliability} message #{newPackage._reliableMessageNumber}, Chan: #{newPackage._orderingChannel}, OrdIdx: #{newPackage._orderingIndex}");
					HandleConnectedPackage(newPackage);
					newPackage.PutPool();
				}
				catch (Exception e)
				{
					Log.Error("Error during split message parsing", e);
					if (Log.IsDebugEnabled)
						Log.Info($"0x{buffer[0]:x2}\n{Package.HexDump(buffer)}");
				}
			}
		}


		public AutoResetEvent FirstEncryptedPacketWaitHandle = new AutoResetEvent(false);

		public AutoResetEvent FirstPacketWaitHandle = new AutoResetEvent(false);

		private void OnWrapper(McpeWrapper batch)
		{
			FirstPacketWaitHandle.Set();

			var messages = new List<Package>();


			// Get bytes
			byte[] payload = batch.payload;

			if (Session.CryptoContext != null && Session.CryptoContext.UseEncryption)
			{
				FirstEncryptedPacketWaitHandle.Set();
				payload = CryptoUtils.Decrypt(payload, Session.CryptoContext);
			}

			MemoryStream stream = new MemoryStream(payload);
			if (stream.ReadByte() != 0x78)
			{
				throw new InvalidDataException("Incorrect ZLib header. Expected 0x78 0x9C");
			}
			stream.ReadByte();
			using (var defStream2 = new DeflateStream(stream, CompressionMode.Decompress, false))
			{
				// Get actual package out of bytes
				using (MemoryStream destination = MiNetServer.MemoryStreamManager.GetStream())
				{
					defStream2.CopyTo(destination);
					destination.Position = 0;
					byte[] internalBuffer = null;
					do
					{
						try
						{
							var len = VarInt.ReadUInt32(destination);
							internalBuffer = new byte[len];
							destination.Read(internalBuffer, 0, (int)len);

							if (internalBuffer[0] == 0x8e) throw new Exception("Wrong code, didn't expect a 0x8E in a batched packet");

							var package = PackageFactory.CreatePackage(internalBuffer[0], internalBuffer, "mcpe") ??
										  new UnknownPackage(internalBuffer[0], internalBuffer);
							messages.Add(package);

							//if (Log.IsDebugEnabled) Log.Info($"Batch: {package.GetType().Name} 0x{package.Id:x2}");
							//if (!(package is McpeFullChunkData)) Log.Info($"Batch: {package.GetType().Name} 0x{package.Id:x2} \n{Package.HexDump(internalBuffer)}");
						}
						catch (Exception e)
						{
							if (internalBuffer != null)
								Log.Error($"Batch error while reading:\n{Package.HexDump(internalBuffer)}");
							Log.Error("Batch processing", e);
							//throw;
						}
					} while (destination.Position < destination.Length);
				}
			}

			if (messages.Count == 0) Log.Error($"Batch had 0 packets.");

			foreach (var msg in messages)
			{
				msg.DatagramSequenceNumber = batch.DatagramSequenceNumber;
				msg.OrderingChannel = batch.OrderingChannel;
				msg.OrderingIndex = batch.OrderingIndex;
				HandlePackage(msg);
				msg.PutPool();
			}
		}

		private void OnMcpeServerToClientHandshake(McpeServerToClientHandshake message)
		{
			/*string token = message.token;
			Log.Debug("JWT:\n" + token);

			IDictionary<string, dynamic> headers = JWT.Headers(token);
			string x5u = headers["x5u"];
			ECDiffieHellmanCngPublicKey newKey = (ECDiffieHellmanCngPublicKey)CryptoUtils.FromDerEncoded(x5u.DecodeBase64Url());
			var data = JWT.Decode<HandshakeData>(token, newKey.Import());

			InitiateEncryption(Base64Url.Decode(x5u), Base64Url.Decode(data.salt));*/
		}

		private void InitiateEncryption(byte[] serverKey, byte[] randomKeyToken)
		{
			try
			{
				ECDiffieHellmanPublicKey publicKey = CryptoUtils.FromDerEncoded(serverKey);
				Log.Debug("ServerKey (b64):\n" + serverKey);
				Log.Debug($"Cert:\n{publicKey.ToXmlString()}");

				Log.Debug($"RANDOM TOKEN (raw):\n\n{Encoding.UTF8.GetString(randomKeyToken)}");

				if (randomKeyToken.Length != 0)
				{
					Log.Error("Lenght of random bytes: " + randomKeyToken.Length);
				}

				// Create shared shared secret
				ECDiffieHellmanCng ecKey = new ECDiffieHellmanCng(Session.CryptoContext.ClientKey);
				ecKey.HashAlgorithm = CngAlgorithm.Sha256;
				ecKey.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
				ecKey.SecretPrepend = randomKeyToken; // Server token

				byte[] secret = ecKey.DeriveKeyMaterial(publicKey);

				//Log.Debug($"SECRET KEY (b64):\n{Convert.ToBase64String(secret)}");
				Log.Debug($"SECRET KEY (raw):\n{Encoding.UTF8.GetString(secret)}");

				{
					RijndaelManaged rijAlg = new RijndaelManaged
					{
						BlockSize = 128,
						Padding = PaddingMode.None,
						Mode = CipherMode.CFB,
						FeedbackSize = 8,
						Key = secret,
						IV = secret.Take(16).ToArray(),
					};

					// Create a decrytor to perform the stream transform.
					ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
					MemoryStream inputStream = new MemoryStream();
					CryptoStream cryptoStreamIn = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);

					ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
					MemoryStream outputStream = new MemoryStream();
					CryptoStream cryptoStreamOut = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);

					Session.CryptoContext = new CryptoContext
					{
						Algorithm = rijAlg,
						Decryptor = decryptor,
						Encryptor = encryptor,
						InputStream = inputStream,
						OutputStream = outputStream,
						CryptoStreamIn = cryptoStreamIn,
						CryptoStreamOut = cryptoStreamOut,
						UseEncryption = true,
					};
				}

				Thread.Sleep(1250);
				McpeClientToServerHandshake magic = new McpeClientToServerHandshake();
				//byte[] encodedMagic = magic.Encode();
				//McpeBatch batch = BatchUtils.CreateBatchPacket(encodedMagic, 0, encodedMagic.Length, CompressionLevel.Fastest, true);
				//batch.Encode();
				SendPackage(magic);
			}
			catch (Exception e)
			{
				Log.Error("Initiate encryption", e);
			}
		}

		public delegate void DisconnectDelegate(string reason);

		public event DisconnectDelegate OnDisconnect;

		private void HandlePackage(Package message)
		{
		 //   Log.InfoFormat("Receivied {0}", message);

			if (typeof(McpeWrapper) == message.GetType())
			{
				OnWrapper((McpeWrapper)message);
				return;
			}

		//	Log.InfoFormat("Got: {0}", message.GetType());

			if (typeof(McpeServerToClientHandshake) == message.GetType())
			{
				OnMcpeServerToClientHandshake((McpeServerToClientHandshake)message);

				return;
			}

			if (typeof(McpeDisconnect) == message.GetType())
			{
				McpeDisconnect msg = (McpeDisconnect)message;
				OnDisconnect?.Invoke(msg.message);
				Disconnect();
				return;
			}

			if (typeof(DisconnectionNotification) == message.GetType())
			{
				OnDisconnect?.Invoke("Host server closed the connection!");
				Disconnect();
				return;
			}

			if (typeof(ConnectedPing) == message.GetType())
			{
				ConnectedPing msg = (ConnectedPing)message;
				SendConnectedPong(msg.sendpingtime);
				return;
			}

			if (typeof(McpeResourcePacksInfo) == message.GetType())
			{
				OnMcpeResourcePacksInfo((McpeResourcePacksInfo)message);
				return;
			}

			if (typeof(McpeResourcePackStack) == message.GetType())
			{
				OnMcpeResourcePackStack((McpeResourcePackStack)message);
				return;
			}

			if (typeof(McpeResourcePackDataInfo) == message.GetType())
			{
				OnMcpeResourcePackDataInfo((McpeResourcePackDataInfo)message);
				return;
			}

			if (typeof(McpeResourcePackChunkData) == message.GetType())
			{
				OnMcpeResourcePackChunkData((McpeResourcePackChunkData)message);
				return;
			}

			if (typeof(McpeFullChunkData) == message.GetType())
			{
				OnFullChunkData(message);
				return;
			}

			if (typeof(ConnectionRequestAccepted) == message.GetType())
			{
				OnConnectionRequestAccepted();
				return;
			}

			if (typeof(McpeSetSpawnPosition) == message.GetType())
			{
				OnMcpeSetSpawnPosition(message);
				return;
			}

			if (typeof(McpeStartGame) == message.GetType())
			{
				OnMcpeStartGame(message);
				return;
			}

			if (typeof(McpeAddPlayer) == message.GetType())
			{
				OnMcpeAddPlayer(message);
				return;
			}

			if (typeof(McpeSetEntityData) == message.GetType())
			{
				OnMcpeSetEntityData(message);
				return;
			}

			if (typeof(McpeMovePlayer) == message.GetType())
			{
				OnMcpeMovePlayer(message);
				return;
			}

			if (typeof(McpeUpdateBlock) == message.GetType())
			{
				OnMcpeUpdateBlock(message);
				return;
			}

			if (typeof(McpeMobEquipment) == message.GetType())
			{
				OnMcpePlayerEquipment((McpeMobEquipment)message);
				return;
			}

			else if (typeof(McpeInventoryContent) == message.GetType())
			{
				OnMcpeInventoryContent((McpeInventoryContent)message);
				return;
			}

			if (typeof(McpeContainerSetData) == message.GetType())
			{
				OnMcpeContainerSetData(message);
				return;
			}

			if (typeof(McpeSetDifficulty) == message.GetType())
			{
				OnSetDifficulty((McpeSetDifficulty)message);
				return;
			}

			if (typeof(McpeUpdateAttributes) == message.GetType())
			{
				OnUpdateAttributes((McpeUpdateAttributes)message);
				return;
			}

			if (typeof(McpeSetTime) == message.GetType())
			{
				OnSetTime((McpeSetTime)message);
				return;
			}

			if (typeof(McpeText) == message.GetType())
			{
				OnMcpeText((McpeText)message);
				return;
			}

			if (typeof(McpeAddEntity) == message.GetType())
			{
				OnAddEntity((McpeAddEntity)message);
				return;
			}

			if (typeof(McpeRemoveEntity) == message.GetType())
			{
				OnRemovePlayer((McpeRemoveEntity)message);
				return;
			}

			if (typeof(McpePlayerList) == message.GetType())
			{
				OnMcpePlayerList?.Invoke((McpePlayerList)message);
				return;
			}

			if (typeof(McpeAnimate) == message.GetType())
			{
				OnMcpeAnimate?.Invoke((McpeAnimate)message);
				return;
			}

			if (typeof(McpeRespawn) == message.GetType())
			{
				OnMcpeRespawn?.Invoke((McpeRespawn)message);
				return;
			}

			if (typeof(McpePlayerAction) == message.GetType())
			{
				OnPlayerAction?.Invoke((McpePlayerAction)message);
				return;
			}

			if (typeof(McpeBlockEntityData) == message.GetType())
			{
				OnTileEntityData?.Invoke((McpeBlockEntityData)message);
				return;
			}

			if (typeof(McpeMobArmorEquipment) == message.GetType())
			{
				OnMcpePlayerArmor?.Invoke((McpeMobArmorEquipment)message);
				return;
			}

			if (typeof(McpeRemoveEntity) == message.GetType())
			{
				OnMcpeRemoveEntity?.Invoke((McpeRemoveEntity)message);
				return;
			}

		
			if (typeof(McpePlayStatus) == message.GetType())
			{
				PlayerStatus = ((McpePlayStatus)message).status;
				OnMcpePlayerStatus?.Invoke((McpePlayStatus)message);
				return;
			}

			if (typeof(McpeAddItemEntity) == message.GetType())
			{
				OnMcpeAddItemEntity?.Invoke((McpeAddItemEntity)message);
				return;
			}

			if (typeof(McpeTakeItemEntity) == message.GetType())
			{
				OnMcpeTakeItemEntity?.Invoke((McpeTakeItemEntity)message);
				return;
			}

			if (typeof(McpeChunkRadiusUpdate) == message.GetType())
			{
				OnMcpeChunkRadiusUpdate?.Invoke((McpeChunkRadiusUpdate)message);
				return;
			}

			if (typeof(McpeAdventureSettings) == message.GetType())
			{
				OnMcpeAdventureSettings?.Invoke((McpeAdventureSettings)message);
				return;
			}

			if (typeof(McpeMoveEntity) == message.GetType())
			{
				HandleMcpeMoveEntity((McpeMoveEntity)message);
				return;
			}

			if (typeof(McpeSetEntityMotion) == message.GetType())
			{
				HandleMcpeSetEntityMotion((McpeSetEntityMotion)message);
				return;
			}

			if (typeof(McpeEntityEvent) == message.GetType())
			{
				OnMcpeEntityEvent?.Invoke((McpeEntityEvent)message);
				return;
			}

			if (typeof(McpeContainerOpen) == message.GetType())
			{
				OnMcpeContainerOpen?.Invoke((McpeContainerOpen)message);
				return;
			}

			if (typeof(McpeBlockEvent) == message.GetType())
			{
				OnMcpeTileEvent?.Invoke((McpeBlockEvent)message);
				return;
			}

			if (typeof(McpeExplode) == message.GetType())
			{
				OnMcpeExplode?.Invoke((McpeExplode)message);
				return;
			}

			if (typeof(McpeMobEffect) == message.GetType())
			{
				OnMcpeMobEffect?.Invoke((McpeMobEffect)message);
				return;
			}

			if (typeof(UnknownPackage) == message.GetType())
			{
				UnknownPackage packet = (UnknownPackage)message;
				if (Log.IsDebugEnabled) Log.Warn($"Unknown package 0x{message.Id:X2}\n{Package.HexDump(packet.Message)}");

				return;
			}

			Log.Warn($"Unhandled package: {message.GetType().Name}");
		}

		public delegate void PacketDelegate<in T>(T packet) where T : Package;

		public event PacketDelegate<McpePlayerList> OnMcpePlayerList;
		public event PacketDelegate<McpeAnimate> OnMcpeAnimate;
		public event PacketDelegate<McpeRespawn> OnMcpeRespawn;
		public event PacketDelegate<McpePlayerAction> OnPlayerAction;
		public event PacketDelegate<McpeBlockEntityData> OnTileEntityData;
		public event PacketDelegate<McpeMobArmorEquipment> OnMcpePlayerArmor;
		public event PacketDelegate<McpeRemoveEntity> OnMcpeRemoveEntity;
		public event PacketDelegate<McpePlayStatus> OnMcpePlayerStatus;
		public event PacketDelegate<McpeAddItemEntity> OnMcpeAddItemEntity;
		public event PacketDelegate<McpeTakeItemEntity> OnMcpeTakeItemEntity;
		public event PacketDelegate<McpeChunkRadiusUpdate> OnMcpeChunkRadiusUpdate;
		public event PacketDelegate<McpeAdventureSettings> OnMcpeAdventureSettings;
		public event PacketDelegate<McpeEntityEvent> OnMcpeEntityEvent;
		public event PacketDelegate<McpeContainerOpen> OnMcpeContainerOpen;
		public event PacketDelegate<McpeBlockEvent> OnMcpeTileEvent;
		public event PacketDelegate<McpeExplode> OnMcpeExplode;
		public event PacketDelegate<McpeMobEffect> OnMcpeMobEffect;

		public delegate void McpeRemovePlayerDelegate(long entityId, UUID clientUuid);

		public event McpeRemovePlayerDelegate OnPlayerRemoval;

		private void OnRemovePlayer(McpeRemoveEntity message)
		{
			OnPlayerRemoval?.Invoke(message.entityIdSelf, null);
		}

		public event PacketDelegate<McpeAddEntity> OnEntityAdd;

		private void OnAddEntity(McpeAddEntity message)
		{
			OnEntityAdd?.Invoke(message);
		}

		public delegate void McpeSetTimeDelegate(int time);

		public event McpeSetTimeDelegate OnTimeChanged;

		private void OnSetTime(McpeSetTime message)
		{
			OnTimeChanged?.Invoke(message.time);
		}

		public event PacketDelegate<McpeUpdateAttributes> OnMcpeUpdateAttributes;
		private void OnUpdateAttributes(McpeUpdateAttributes message)
		{
			OnMcpeUpdateAttributes?.Invoke(message);
		}

		public delegate void SetDifficultyDelegate(Difficulty difficulty);

		public event SetDifficultyDelegate OnServerDifficulty;

		private void OnSetDifficulty(McpeSetDifficulty message)
		{
			OnServerDifficulty?.Invoke((Difficulty)message.difficulty);
		}

		public delegate void McpeTextDelegate(string message, string source, MessageType type);

		public event McpeTextDelegate OnChatMessage;

		private void OnMcpeText(McpeText message)
		{
			OnChatMessage?.Invoke(message.message, message.source, (MessageType)message.type);
		}

		public delegate void McpePlayerEquipmentDelegate(long entityid, Item item);

		public event McpePlayerEquipmentDelegate OnPlayerEquipment;

		private void OnMcpePlayerEquipment(McpeMobEquipment message)
		{
			OnPlayerEquipment?.Invoke(message.runtimeEntityId, message.item);
		}

		public delegate void McpeContainerSetDataDelegate(byte windowId, int value, int property);

		public event McpeContainerSetDataDelegate OnContainerSetData;

		private void OnMcpeContainerSetData(Package msg)
		{
			McpeContainerSetData message = (McpeContainerSetData)msg;
			OnContainerSetData?.Invoke(message.windowId, message.value, message.property);
		}

		/*public event PacketDelegate<McpeContainerSetSlot> OnSetContainerSlot;

		private void OnMcpeContainerSetSlot(Package msg)
		{
			McpeContainerSetSlot message = (McpeContainerSetSlot)msg;
			OnSetContainerSlot?.Invoke(message);
		}

		public event PacketDelegate<McpeContainerSetContent> OnContainerContent;

		private void OnMcpeContainerSetContent(Package message)
		{
			McpeContainerSetContent msg = (McpeContainerSetContent)message;
			OnContainerContent?.Invoke(msg);
		}*/

		private void OnMcpeInventoryContent(McpeInventoryContent msg)
		{
			
		}

		public event PacketDelegate<McpeUpdateBlock> OnBlockUpdate;

		private void OnMcpeUpdateBlock(Package message)
		{
			McpeUpdateBlock msg = (McpeUpdateBlock)message;
			OnBlockUpdate?.Invoke(msg);
		}

		public event PacketDelegate<McpeMovePlayer> OnPlayerMovement;

		private void OnMcpeMovePlayer(Package message)
		{
			McpeMovePlayer msg = (McpeMovePlayer)message;

			OnPlayerMovement?.Invoke(msg);
		}

		public event PacketDelegate<McpeSetEntityData> OnEntityData;
		private void OnMcpeSetEntityData(Package message)
		{
			McpeSetEntityData msg = (McpeSetEntityData)message;
			OnEntityData?.Invoke(msg);
		}

		public event PacketDelegate<McpeAddPlayer> OnPlayerAdd;

		private void OnMcpeAddPlayer(Package message)
		{
			McpeAddPlayer msg = (McpeAddPlayer)message;
			OnPlayerAdd?.Invoke(msg);
		}

		public delegate void McpeStartGameDelegate(GameMode gamemode, Vector3 spawnPoint, long entityId);

		public event McpeStartGameDelegate OnStartGame;

		private void OnMcpeStartGame(Package message)
		{
			McpeStartGame msg = (McpeStartGame)message;
			OnStartGame?.Invoke((GameMode)msg.gamemode, new Vector3(msg.x, msg.y, msg.z), msg.runtimeEntityId);
		}

		public delegate void McpeSetSpawnPositionDelegate(Vector3 location);

		public event McpeSetSpawnPositionDelegate OnSetSpawnPosition;

		private void OnMcpeSetSpawnPosition(Package message)
		{
			McpeSetSpawnPosition msg = (McpeSetSpawnPosition)message;
			
			OnSetSpawnPosition?.Invoke(new Vector3(msg.coordinates.X, msg.coordinates.Y, msg.coordinates.Z));
		}

		private void OnConnectionRequestAccepted()
		{
			Thread.Sleep(50);
			SendNewIncomingConnection();
			//_connectedPingTimer = new Timer(state => SendConnectedPing(), null, 1000, 1000);
			Thread.Sleep(50);
			SendLogin(Username);
		}

		private void OnMcpeResourcePacksInfo(McpeResourcePacksInfo message)
		{
			Log.Warn($"HEX: \n{Package.HexDump(message.Bytes)}");

			var sb = new StringBuilder();
			sb.AppendLine();

			sb.AppendLine("Resource packs:");
			foreach (ResourcePackInfo info in message.resourcepackinfos)
			{
				sb.AppendLine($"ID={info.PackIdVersion.Id}, Version={info.PackIdVersion.Version}, Unknown={info.Size}");
			}

			sb.AppendLine("Behavior packs:");
			foreach (ResourcePackInfo info in message.behahaviorpackinfos)
			{
				sb.AppendLine($"ID={info.PackIdVersion.Id}, Version={info.PackIdVersion.Version}");
			}

			Log.Info(sb.ToString());

			//McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
			//response.responseStatus = 3;
			//SendPackage(response);

			if (message.resourcepackinfos.Count != 0)
			{
				ResourcePackIds resourcePackIds = new ResourcePackIds();

				foreach (var packInfo in message.resourcepackinfos)
				{
					resourcePackIds.Add(packInfo.PackIdVersion.Id);
				}

				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = 2;
				response.resourcepackids = resourcePackIds;
				SendPackage(response);
			}
			else
			{
				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = 3;
				SendPackage(response);
			}
		}

		private void OnMcpeResourcePackStack(McpeResourcePackStack message)
		{
			Log.Info($"HEX: \n{Package.HexDump(message.Bytes)}");

			var sb = new StringBuilder();
			sb.AppendLine();

			sb.AppendLine("Resource pack stacks:");
			foreach (var info in message.resourcepackidversions)
			{
				sb.AppendLine($"ID={info.Id}, Version={info.Version}");
			}

			sb.AppendLine("Behavior pack stacks:");
			foreach (var info in message.behaviorpackidversions)
			{
				sb.AppendLine($"ID={info.Id}, Version={info.Version}");
			}

			Log.Info(sb.ToString());

			//if (message.resourcepackidversions.Count != 0)
			//{
			//	McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
			//	response.responseStatus = 2;
			//	response.resourcepackidversions = message.resourcepackidversions;
			//	SendPackage(response);
			//}
			//else
			{
				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = 4;
				SendPackage(response);
			}

		}

		private Dictionary<string, uint> resourcePackDataInfos = new Dictionary<string, uint>();
		private void OnMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
		{
			var packageId = message.packageId;
			McpeResourcePackChunkRequest request = new McpeResourcePackChunkRequest();
			request.packageId = packageId;
			request.chunkIndex = 0;
			SendPackage(request);
		}

		private void OnMcpeResourcePackChunkData(McpeResourcePackChunkData message)
		{
			string fileName = Path.GetTempPath() + "ResourcePackChunkData_" + message.packageId + ".zip";
			Log.Warn("Writing ResourcePackChunkData part " + message.chunkIndex.ToString() + " to filename: " + fileName);

			FileStream file = File.OpenWrite(fileName);
			file.Seek((long)message.progress, SeekOrigin.Begin);

			file.Write(message.payload, 0, message.payload.Length);
			file.Close();

			Log.Info($"packageId={message.packageId}");
			Log.Info($"unknown1={message.chunkIndex}");
			Log.Info($"unknown3={message.progress}");
			Log.Info($"Reported Lenght={message.length}");
			Log.Info($"Actual Lenght={message.payload.Length}");

			if (message.chunkIndex + 1 < resourcePackDataInfos[message.packageId])
			{
				var packageId = message.packageId;
				McpeResourcePackChunkRequest request = new McpeResourcePackChunkRequest();
				request.packageId = packageId;
				request.chunkIndex = message.chunkIndex + 1;
				SendPackage(request);
			}
			else
			{
				resourcePackDataInfos.Remove(message.packageId);
			}

			if (resourcePackDataInfos.Count == 0)
			{
				McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
				response.responseStatus = 3;
				SendPackage(response);
			}
		}

		public delegate void FullChunkDataDelegate(ChunkColumn chunkColumn);
		public event FullChunkDataDelegate OnChunkData;

		private void OnFullChunkData(Package message)
		{
			McpeFullChunkData msg = (McpeFullChunkData)message;
			try
			{
				ChunkColumn chunkColumn = new ChunkColumn();

				using (MemoryStream stream = new MemoryStream(msg.chunkData))
				{
					NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

					Log.Debug("New chunk column");

					int count = defStream.ReadByte();
					if (count < 1)
					{
						Log.Warn("Nothing to read");
						return;
					}

					Log.Debug($"Reading {count} sections");

					for (int s = 0; s < count; s++)
					{
						int idx = defStream.ReadByte();

						Log.Debug($"New section {s}, index={idx}");
						Chunk chunk = chunkColumn.chunks[s];

						int chunkSize = 16 * 16 * 16;
						defStream.Read(chunk.blocks, 0, chunkSize);

						if (defStream.Read(chunk.metadata.Data, 0, chunkSize / 2) != chunkSize / 2) Log.Error($"Out of data: metadata");
					}


					byte[] ba = new byte[512];
					if (defStream.Read(ba, 0, 256 * 2) != 256 * 2) Log.Error($"Out of data height");

					Buffer.BlockCopy(ba, 0, chunkColumn.height, 0, 512);
					//Log.Debug($"Heights:\n{Package.HexDump(ba)}");

					//if (stream.Position >= stream.Length - 1) continue;

					if (defStream.Read(chunkColumn.biomeId, 0, 256) != 256) Log.Error($"Out of data biomeId");
					//Log.Debug($"biomeId:\n{Package.HexDump(chunk.biomeId)}");

					//if (stream.Position >= stream.Length - 1) continue;


					int borderBlock = VarInt.ReadInt32(stream);
					if (borderBlock != 0)
					{
						byte[] buf = new byte[borderBlock];
						int len = defStream.Read(buf, 0, borderBlock);
						Log.Warn($"??? Got borderblock {borderBlock}. Read {len} bytes");
						Log.Debug($"{Package.HexDump(buf)}");
						for (int i = 0; i < borderBlock; i++)
						{
							int x = (buf[i] & 0xf0) >> 4;
							int z = buf[i] & 0x0f;
							Log.Debug($"x={x}, z={z}");
						}
					}

					int extraCount = VarInt.ReadSInt32(stream);
					if (extraCount != 0)
					{
						//Log.Warn($"Got extradata\n{Package.HexDump(defStream.ReadBytes(extraCount*10))}");
						for (int i = 0; i < extraCount; i++)
						{
							var hash = VarInt.ReadSInt32(stream);
							var blockData = defStream.ReadInt16();
							Log.Warn($"Got extradata: hash=0x{hash:X2}, blockdata=0x{blockData:X2}");
						}
					}

					if (stream.Position < stream.Length - 1)
					{
						//Log.Debug($"Got NBT data\n{Package.HexDump(defStream.ReadBytes((int) (stream.Length - stream.Position)))}");

						while (stream.Position < stream.Length)
						{
							NbtFile file = new NbtFile() { BigEndian = false, UseVarInt = true };

							file.LoadFromStream(stream, NbtCompression.None);

							Log.Debug($"Blockentity: {file.RootTag}");
						}
					}
					if (stream.Position < stream.Length - 1)
					{
						Log.Warn($"Still have data to read\n{Package.HexDump(defStream.ReadBytes((int)(stream.Length - stream.Position)))}");
					}

					if (chunkColumn != null)
					{
						chunkColumn.x = msg.chunkX;
						chunkColumn.z = msg.chunkZ;
						Log.InfoFormat("Chunk X={0}, Z={1}", chunkColumn.x, chunkColumn.z);
						foreach (KeyValuePair<BlockCoordinates, NbtCompound> blockEntity in chunkColumn.BlockEntities)
						{
							Log.Info($"Blockentity: {blockEntity.Value}");
						}

						OnChunkData?.Invoke(chunkColumn);
						//ClientUtils.SaveChunkToAnvil(chunk);
					}
				}
			}
			catch (Exception e)
			{
				Log.Error("Reading chunk", e);
			}
		}

		public event PacketDelegate<McpeSetEntityMotion> OnMcpeSetEntityMotion;
		private void HandleMcpeSetEntityMotion(McpeSetEntityMotion packet)
		{
		  /*  var count = packet.ReadInt();
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					var key = packet.ReadLong();
					var x = packet.ReadFloat();
					var y = packet.ReadFloat();
					var z = packet.ReadFloat();
					packet.entities.Add(key, new Vector3(x, y, z));
				}
			}

			OnMcpeSetEntityMotion?.Invoke(packet);*/
		}

		public event PacketDelegate<McpeMoveEntity> OnMcpeMoveEntity;
		private void HandleMcpeMoveEntity(McpeMoveEntity packet)
		{
			OnMcpeMoveEntity?.Invoke(packet);
		   /* EntityLocations el = new EntityLocations();

			var count = packet.ReadInt();
			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					var key = packet.ReadLong();
					var x = packet.ReadFloat();
					var y = packet.ReadFloat();
					var z = packet.ReadFloat();
					var yaw = packet.ReadFloat();
					var headYaw = packet.ReadFloat();
					var pitch = packet.ReadFloat();

					el.Add(key, new PlayerLocation(x, y, z)
					{
						HeadYaw = headYaw,
						Yaw = yaw,
						Pitch = pitch
					});
				}
			}

			packet.entities = el;
			OnMcpeMoveEntity?.Invoke(packet);*/
		}

		public void SendUnconnectedPing()
		{
			var packet = new UnconnectedPing
			{
				pingId = DateTime.UtcNow.Ticks /*incoming.pingId*/,
			};

			var data = packet.Encode();
			//TraceSend(packet);
			//SendData(data);
			SendData(data);
		}

		public void SendConnectedPing()
		{
			var packet = new ConnectedPing()
			{
				sendpingtime = DateTime.UtcNow.Ticks
			};

			SendPackage(packet);
		}

		private void SendConnectedPong(long sendpingtime)
		{
			SendPackage(new ConnectedPong
			{
				sendpingtime = sendpingtime,
				sendpongtime = sendpingtime + 10
			});
		}

		private void SendOpenConnectionRequest1()
		{
			var packet = new OpenConnectionRequest1
			{
				raknetProtocolVersion = 8,
				mtuSize = _mtuSize
			};

			byte[] data = packet.Encode();

			// 1087 1447
			byte[] data2 = new byte[_mtuSize - data.Length];
			Buffer.BlockCopy(data, 0, data2, 0, data.Length);

			SendData(data2);
		}

		private void SendOpenConnectionRequest2()
		{
			_clientGuid = new Random().Next() + new Random().Next();
			var packet = new OpenConnectionRequest2()
			{
				mtuSize = _mtuSize,
				clientGuid = _clientGuid,
				remoteBindingAddress = _clientEndpoint
			};

			var data = packet.Encode();

			SendData(data);
		}

		private void SendConnectionRequest()
		{
			var packet = new ConnectionRequest
			{
				clientGuid = _clientGuid,
				timestamp = DateTime.UtcNow.Ticks,
				doSecurity = 0
			};

			SendPackage(packet);
		}

		private void SendNewIncomingConnection()
		{
			Random rand = new Random();
			var packet = NewIncomingConnection.CreateObject();
			packet.clientendpoint = _serverEndpoint;
			packet.systemAddresses = new IPEndPoint[20];
			for (int i = 0; i < 20; i++)
			{
				packet.systemAddresses[i] = new IPEndPoint(IPAddress.Any, 0);
			}

			SendPackage(packet);
		}

		public void SendLogin(string username)
		{
			JWT.JsonMapper = new NewtonsoftMapper();

			CngKey clientKey = CryptoUtils.GenerateClientKey();
			byte[] data = CryptoUtils.CompressJwtBytes(CryptoUtils.EncodeJwt(Username, clientKey, true), CryptoUtils.EncodeSkinJwt(clientKey), CompressionLevel.Fastest);

			McpeLogin loginPacket = new McpeLogin
			{
				protocolVersion = Config.GetProperty("EnableEdu", false) ? 111 : 201,
				payload = data
			};

			Session.CryptoContext = new CryptoContext()
			{
				ClientKey = clientKey,
				UseEncryption = false,
			};

			SendPackage(loginPacket);
			//SendPackage(batch);
		}
		public void SendChat(string text)
		{
			SendPackage(new McpeText
			{
				source = Username,
				message = text
			});
		}

		public void SendDisconnectionNotification()
		{
			SendPackage(new DisconnectionNotification());
		}

		public void SendMovePlayer(PlayerLocation location)
		{
			McpeMovePlayer movePlayerPacket = McpeMovePlayer.CreateObject();
			movePlayerPacket.x = location.X;
			movePlayerPacket.y = location.Y;
			movePlayerPacket.z = location.Z;
			movePlayerPacket.yaw = location.Yaw;
			movePlayerPacket.headYaw = location.Yaw;
			movePlayerPacket.pitch = location.Pitch;
		   // movePlayerPacket.entityId 
			SendPackage(movePlayerPacket);
		}

		public void SendAnimation(byte actionId)
		{
			McpeAnimate package = McpeAnimate.CreateObject();
			package.actionId = actionId;
			package.runtimeEntityId = 0;
			SendPackage(package);
		}

		public void RequestRespawn()
		{
			SendAction(PlayerAction.Respawn);
		}

		public void SendAction(PlayerAction action)
		{
			McpePlayerAction packet = McpePlayerAction.CreateObject();
			packet.actionId = (int)action;
			packet.runtimeEntityId = 0;
			packet.face = 0;
			packet.coordinates = new BlockCoordinates(0,0,0);
			SendPackage(packet);
		}

		public void SendPackage(Package package)
		{
			SendPackage(package, _mtuSize);
			package.PutPool();
		}

		private void SendPackage(Package message, short mtuSize)
		{
			if (message == null) return;

		//	Log.Info($"Sending packet: {message.ToString()}");
			foreach (var datagram in Datagram.CreateDatagrams(message, mtuSize, Session))
			{
				SendDatagram(Session, datagram);
			}
		}

		private void SendDatagram(PlayerNetworkSession session, Datagram datagram)
		{
			if (datagram.MessageParts.Count != 0)
			{
				datagram.Header.datagramSequenceNumber = Interlocked.Increment(ref Session.DatagramSequenceNumber);
				byte[] data = datagram.Encode();
				datagram.PutPool();

				SendData(data, _serverEndpoint);
			}
		}

		private void SendData(byte[] data)
		{
			SendData(data, _serverEndpoint);
		}

		private void SendData(byte[] data, IPEndPoint targetEndpoint)
		{
			if (UdpClient == null) return;
			//Logging.Info("Sending " + data.Length + " bytes");
			try
			{
				UdpClient.Send(data, data.Length, targetEndpoint);
			}
			catch (Exception e)
			{
				Log.Warn("Send exception: " + e);
			}
		}
	}
}