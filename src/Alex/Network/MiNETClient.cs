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
        private short _mtuSize = 1447;
        private int ClientId { get; }

        private PlayerNetworkSession Session { get; set; }

        private int _clientGuid;
		public bool HaveServer = false;
		private UdpClient UdpClient { get; set; }

        private string Username { get; set; }
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

                UdpClient.BeginReceive(ReceiveCallback, UdpClient);
                _clientEndpoint = (IPEndPoint)UdpClient.Client.LocalEndPoint;

                SendUnconnectedPing();

				SendOpenConnectionRequest1();
				Task.Run(ProcessQueue);
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
                    //Log.Debug($"> Datagram #{header.datagramSequenceNumber}, {package._hasSplit}, {package._splitPacketId}, {package._reliability}, {package._reliableMessageNumber}, {package._sequencingIndex}, {package._orderingChannel}, {package._orderingIndex}");

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
                Log.DebugFormat("Got all {0} split packages for split ID: {1}", spCount, spId);

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

                    Log.Debug($"Assembled split package {newPackage._reliability} message #{newPackage._reliableMessageNumber}, Chan: #{newPackage._orderingChannel}, OrdIdx: #{newPackage._orderingIndex}");
                    HandleConnectedPackage(newPackage);
                    newPackage.PutPool();
                }
                catch (Exception e)
                {
                    Log.Error("Error during split message parsing", e);
                    if (Log.IsDebugEnabled)
                        Log.Debug($"0x{buffer[0]:x2}\n{Package.HexDump(buffer)}");
                }
            }
        }


        public AutoResetEvent FirstEncryptedPacketWaitHandle = new AutoResetEvent(false);

        public AutoResetEvent FirstPacketWaitHandle = new AutoResetEvent(false);

        private void OnWrapper(McpeWrapper message)
		{
            FirstPacketWaitHandle.Set();

            // Get bytes
            byte[] payload = message.payload;
            if (Session.CryptoContext != null && Session.CryptoContext.UseEncryption)
            {
                FirstEncryptedPacketWaitHandle.Set();
                payload = CryptoUtils.Decrypt(payload, Session.CryptoContext);
            }

            //if (Log.IsDebugEnabled)
            //Log.Debug($"0x{payload[0]:x2}\n{Package.HexDump(payload)}");

            try
            {
                Package newMessage = PackageFactory.CreatePackage(payload[0], payload, "mcpe") ?? new UnknownPackage(payload[0], payload);
                //TraceReceive(newMessage);

                if (_processingThread == null)
                {
                    HandlePackage(newMessage);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(state => HandlePackage(newMessage));
                }
            }
            catch (Exception e)
            {
                Log.Error("Wrapper", e);
            }

            //Task.Run(() => { HandlePackage(newMessage); });
        }

        private void OnMcpeServerExchange(McpeServerExchange message)
		{
			string serverKey = message.serverPublicKey;
			byte[] randomKeyToken = message.token;

			// Initiate encryption

			InitiateEncryption(serverKey, randomKeyToken);
		}

		private void InitiateEncryption(string serverKey, byte[] randomKeyToken)
		{
            {
                ECDiffieHellmanPublicKey publicKey = CryptoUtils.CreateEcDiffieHellmanPublicKey(serverKey);
              //  Log.Debug("ServerKey (b64):\n" + serverKey);
                //Log.Debug($"Cert:\n{publicKey.ToXmlString()}");

              //  Log.Debug($"RANDOM TOKEN (raw):\n{randomKeyToken}");

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
              //  Log.Debug($"SECRET KEY (raw):\n{Encoding.UTF8.GetString(secret)}");

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

               // Thread.Sleep(1250);
                McpeClientMagic magic = new McpeClientMagic();
                byte[] encodedMagic = magic.Encode();
                McpeBatch batch = BatchUtils.CreateBatchPacket(encodedMagic, 0, encodedMagic.Length, CompressionLevel.Fastest, true);
                batch.Encode();
                SendPackage(batch);
            }
        }

		public delegate void DisconnectDelegate(string reason);

        public event DisconnectDelegate OnDisconnect;

		private void HandlePackage(Package message)
        {
         //   Log.DebugFormat("Receivied {0}", message);

			if (typeof(McpeWrapper) == message.GetType())
			{
				OnWrapper((McpeWrapper)message);
				return;
			}

			if (typeof(McpeServerExchange) == message.GetType())
			{
				OnMcpeServerExchange((McpeServerExchange)message);

				return;
			}

			if (typeof(McpeBatch) == message.GetType())
            {
                OnBatch(message);
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

            if (typeof(McpeContainerSetContent) == message.GetType())
            {
                OnMcpeContainerSetContent(message);
                return;
            }

            if (typeof(McpeContainerSetSlot) == message.GetType())
            {
                OnMcpeContainerSetSlot(message);
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

            if (typeof(McpePlayerStatus) == message.GetType())
            {
				PlayerStatus = ((McpePlayerStatus)message).status;
				OnMcpePlayerStatus?.Invoke((McpePlayerStatus)message);
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
        public event PacketDelegate<McpePlayerStatus> OnMcpePlayerStatus;
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
            OnPlayerRemoval?.Invoke(message.entityId, null);
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
            OnPlayerEquipment?.Invoke(message.entityId, message.item);
        }

        public delegate void McpeContainerSetDataDelegate(byte windowId, int value, int property);

        public event McpeContainerSetDataDelegate OnContainerSetData;

        private void OnMcpeContainerSetData(Package msg)
        {
            McpeContainerSetData message = (McpeContainerSetData)msg;
            OnContainerSetData?.Invoke(message.windowId, message.value, message.property);
        }

        public event PacketDelegate<McpeContainerSetSlot> OnSetContainerSlot;

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
            OnStartGame?.Invoke((GameMode)msg.gamemode, new Vector3(msg.x, msg.y, msg.z), msg.entityId);
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
            Log.Debug($"HEX: \n{Package.HexDump(message.Bytes)}");

            var sb = new StringBuilder();
            sb.AppendLine();

            sb.AppendLine("Resource packs:");
            foreach (ResourcePackInfo info in message.resourcepackinfos)
            {
                sb.AppendLine($"ID={info.PackIdVersion.Id}, Version={info.PackIdVersion.Version}, Unknown={info.Unknown}");
            }

            sb.AppendLine("Behavior packs:");
            foreach (ResourcePackInfo info in message.behahaviorpackinfos)
            {
                sb.AppendLine($"ID={info.PackIdVersion.Id}, Version={info.PackIdVersion.Version}");
            }

            Log.Debug(sb.ToString());

            //McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
            //response.responseStatus = 3;
            //SendPackage(response);

            if (message.resourcepackinfos.Count != 0)
            {
                ResourcePackIdVersions resourceInfos = new ResourcePackIdVersions();
                foreach (var packInfo in message.resourcepackinfos)
                {
                    resourceInfos.Add(packInfo.PackIdVersion);
                }

                McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
                response.responseStatus = 2;
                response.resourcepackidversions = resourceInfos;
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
            Log.Debug($"HEX: \n{Package.HexDump(message.Bytes)}");

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

            Log.Debug(sb.ToString());

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
            //string fileName = Path.GetTempPath() + "ResourcePackChunkData_" + Guid.NewGuid() + ".zip";
            //Log.Info("Writing ResourcePackChunkData to filename: " + fileName);
            //FileStream file = File.OpenWrite(fileName);
            //file.Write(message.payload, 0, message.payload.Length);
            //file.Close();

            Log.Debug($"packageId={message.packageId}");
            Log.Debug($"unknown1={message.unknown1}");
            Log.Debug($"unknown3={message.unknown3}");
            Log.Debug($"Reported Lenght={message.length}");
            Log.Debug($"Actual Lenght={message.payload.Length}");

            McpeResourcePackClientResponse response = new McpeResourcePackClientResponse();
            response.responseStatus = 3;
            SendPackage(response);
        }

        public delegate void FullChunkDataDelegate(ChunkColumn chunkColumn);
        public event FullChunkDataDelegate OnChunkData;

        private void OnFullChunkData(Package message)
        {
            McpeFullChunkData msg = (McpeFullChunkData)message;
            try
            {
                //ChunkColumn chunk = new ChunkColumn();
                using (MemoryStream stream = new MemoryStream(msg.chunkData))
                {
                    NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

                    int count = defStream.ReadByte();
                    if (count < 1)
                    {
                        Log.Warn("Nothing to read");
                        return;
                    }

                    ChunkColumn chunkColumn = new ChunkColumn();


                    for (int s = 0; s < count; s++)
                    {
                        int idx = defStream.ReadByte();

                        Log.Debug($"New section {s}, index={idx}");
                        Chunk chunk = chunkColumn.chunks[s];

                        int chunkSize = 16 * 16 * 16;
                        defStream.Read(chunk.blocks, 0, chunkSize);

                        if (defStream.Read(chunk.metadata.Data, 0, chunkSize / 2) != chunkSize / 2) Log.Error($"Out of data: metadata");

                        if (defStream.Read(chunk.skylight.Data, 0, chunkSize / 2) != chunkSize / 2) Log.Error($"Out of data: skylight");

                        if (defStream.Read(chunk.blocklight.Data, 0, chunkSize / 2) != chunkSize / 2) Log.Error($"Out of data: blocklight");
                    }

                    if (defStream.Read(chunkColumn.height, 0, 256 * 2) != 256 * 2) Log.Error($"Out of data height");

                    if (defStream.Read(chunkColumn.biomeId, 0, 256) != 256) Log.Error($"Out of data biomeId");

                    int extraSize = defStream.ReadInt16();
                    if (extraSize != 0)
                    {
                        Log.Debug($"Got extradata\n{Package.HexDump(defStream.ReadBytes(extraSize))}");
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


                        if (stream.Position < stream.Length - 1)
                        {
                            Log.Debug($"Got data to read\n{Package.HexDump(defStream.ReadBytes((int)(stream.Length - stream.Position)))}");
                        }
                    }

                    if (stream.Position >= stream.Length - 1)
                    {
                    }
                    else
                    {

                        //Log.Debug($"Got NBT data\n{Package.HexDump(defStream.ReadBytes((int) (stream.Length - stream.Position)))}");

                        while (stream.Position < stream.Length)
                        {
                            NbtFile file = new NbtFile() { BigEndian = false, UseVarInt = true };

                            file.LoadFromStream(stream, NbtCompression.None);

                            //Log.Debug($"Blockentity: {file.RootTag}");
                        }
                    }

                    chunkColumn.x = msg.chunkX;
                    chunkColumn.z = msg.chunkZ;
                    OnChunkData?.Invoke(chunkColumn);
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

		private void OnBatch(Package message)
		{
            McpeBatch batch = (McpeBatch)message;

            var messages = new List<Package>();

            // Get bytes
            byte[] payload = batch.payload;
            // Decompress bytes

            MemoryStream stream = new MemoryStream(payload);
            if (stream.ReadByte() != 0x78)
            {
                throw new InvalidDataException("Incorrect ZLib header. Expected 0x78 0x9C");
            }
            stream.ReadByte();
            using (var defStream2 = new DeflateStream(stream, CompressionMode.Decompress, false))
            {
                // Get actual package out of bytes
                MemoryStream destination = MiNetServer.MemoryStreamManager.GetStream();
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

                        var package = PackageFactory.CreatePackage(internalBuffer[0], internalBuffer, "mcpe") ?? new UnknownPackage(internalBuffer[0], internalBuffer);
                        messages.Add(package);

                        //if (Log.IsDebugEnabled) Log.Debug($"Batch: {package.GetType().Name} 0x{package.Id:x2}");
                        //if (!(package is McpeFullChunkData)) Log.Debug($"Batch: {package.GetType().Name} 0x{package.Id:x2} \n{Package.HexDump(internalBuffer)}");
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

            //Log.Error($"Batch had {messages.Count} packets.");
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

		private void SendPackage(Package message, short mtuSize)
        {
			if (message == null) return;

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

        public void SendPackage(Package package)
        {
            SendPackage(package, _mtuSize);
            package.PutPool();
        }

        private void SendNewIncomingConnection()
        {
            var packet = NewIncomingConnection.CreateObject();
            packet.clientendpoint = _clientEndpoint;
            packet.systemAddresses = new IPEndPoint[10];
            for (int i = 0; i < 10; i++)
            {
                packet.systemAddresses[i] = new IPEndPoint(IPAddress.Any, 0);
            }

            SendPackage(packet);
        }

		public void SendLogin(string username)
		{
            //McpeLogin loginPacket = new McpeLogin
            //{
            //	protocolVersion = 81,
            //	payloadLenght = 5617,
            //	payload = Convert.FromBase64String("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")
            //};
            JWT.JsonMapper = new NewtonsoftMapper();

            CngKey clientKey = CryptoUtils.GenerateClientKey();
            byte[] data = CryptoUtils.CompressJwtBytes(CryptoUtils.EncodeJwt(Username, clientKey), CryptoUtils.EncodeSkinJwt(clientKey), CompressionLevel.Fastest);

            McpeLogin loginPacket = new McpeLogin
            {
                protocolVersion = 100,
                edition = 0,
                payload = data
            };

            //var encodedLoginPacket = loginPacket.Encode();
            //McpeBatch batch = Player.CreateBatchPacket(encodedLoginPacket, 0, encodedLoginPacket.Length, CompressionLevel.Fastest, true);
            //batch.Encode();

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
            package.entityId = 0;
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
            packet.entityId = 0;
            packet.face = 0;
            packet.coordinates = new BlockCoordinates(0,0,0);
            SendPackage(packet);
        }
    }
}