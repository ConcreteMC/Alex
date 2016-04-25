using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using fNbt;
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

        private UdpClient UdpClient { get; set; }

        private string Username { get; set; }

        public MiNetClient(IPEndPoint endpoint, string username)
        {
            Username = username;
            ClientId = new Random().Next();
            _serverEndpoint = endpoint;
            _clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
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
                UdpClient = new UdpClient(_clientEndpoint)
                {
                    Client =
                    {
                        ReceiveBufferSize = int.MaxValue,
                        SendBufferSize = int.MaxValue
                    },
                    DontFragment = false
                };

                Session = new PlayerNetworkSession(null, _clientEndpoint, 1300);

                UdpClient.BeginReceive(ReceiveCallback, UdpClient);
                _clientEndpoint = (IPEndPoint)UdpClient.Client.LocalEndPoint;

                SendOpenConnectionRequest1();
                Log.InfoFormat("{0} connected to {1}", _clientEndpoint.ToString(), _serverEndpoint.ToString());

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

        /// <summary>
        ///     Handles the callback.
        /// </summary>
        /// <param name="ar">The results</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
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
                Log.Debug(e);
                try
                {
                    listener.BeginReceive(ReceiveCallback, listener);
                }
                catch (ObjectDisposedException dex)
                {
                    // Log and move on. Should probably free up the player and remove them here.
                    Log.Debug(dex);
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
                    Log.Debug("Processing", e);
                }
            }
            else
            {
                Log.Debug("Unexpected end of transmission?");
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

                Package message = PackageFactory.CreatePackage(msgId, receiveBytes);

                if (message == null) return;

                switch (msgIdType)
                {
                    case DefaultMessageIdTypes.ID_UNCONNECTED_PONG:
                        {
                            SendOpenConnectionRequest1();

                            break;
                        }
                    case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_1:
                        {
                            OpenConnectionReply1 incoming = (OpenConnectionReply1)message;
                            _mtuSize = incoming.mtuSize;
                            SendOpenConnectionRequest2();
                            break;
                        }
                    case DefaultMessageIdTypes.ID_OPEN_CONNECTION_REPLY_2:
                        {
                            SendConnectionRequest();
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

                    ConnectedPackage package = ConnectedPackage.CreateObject();
                    package.Decode(receiveBytes);
                    header = package._datagramHeader;

                    var messages = package.Messages;

                    //ACKS
                    Acks ack = Acks.CreateObject();
                    ack.acks.Add(package._datagramSequenceNumber.IntValue());
                    byte[] data = ack.Encode();
                    SendData(data, senderEndpoint);
                    //END ACKS

                    foreach (var message in messages)
                    {
                        if (message is SplitPartPackage)
                        {
                            lock (Session.SyncRoot)
                            {
                                HandleSplitMessage(Session, package, (SplitPartPackage)message);
                            }

                            continue;
                        }

                        message.Timer.Restart();
                        HandlePackage(message);
                        message.PutPool();
                    }
                    package.PutPool();
                }
                else if (header.isPacketPair)
                {
                    Log.Warn("!! Packet Pair");
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
                    Log.Warn("!! Invalid header");
                }
                else
                {
                    Log.Warn("!! Unknown");
                }
            }
        }


        private void HandleAck(byte[] receiveBytes, IPEndPoint senderEndpoint)
        {

        }

        private void HandleNak(byte[] receiveBytes, IPEndPoint senderEndpoint)
        {

        }

        private void HandleSplitMessage(PlayerNetworkSession playerSession, ConnectedPackage package,
            SplitPartPackage splitMessage)
        {
            int spId = package._splitPacketId;
            int spIdx = package._splitPacketIndex;
            int spCount = package._splitPacketCount;

            if (!playerSession.Splits.ContainsKey(spId))
            {
                playerSession.Splits.TryAdd(spId, new SplitPartPackage[spCount]);
            }

            SplitPartPackage[] spPackets = playerSession.Splits[spId];
            spPackets[spIdx] = splitMessage;

            bool haveEmpty = spPackets.Aggregate(false, (current, t) => current || t == null);

            if (haveEmpty) return;

            SplitPartPackage[] waste;
            playerSession.Splits.TryRemove(spId, out waste);

            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                foreach (SplitPartPackage splitPartPackage in spPackets)
                {
                    byte[] buf = splitPartPackage.Message;
                    stream.Write(buf, 0, buf.Length);
                    splitPartPackage.PutPool();
                }

                buffer = stream.ToArray();
            }

            byte id = buffer[0];
            if (id == 0x8e)
            {
                id = buffer[1];
            }

            Package fullMessage = PackageFactory.CreatePackage(id, buffer) ?? new UnknownPackage(id, buffer);
            fullMessage.DatagramSequenceNumber = package._datagramSequenceNumber;
            fullMessage.OrderingChannel = package._orderingChannel;
            fullMessage.OrderingIndex = package._orderingIndex;

            HandlePackage(fullMessage);
            fullMessage.PutPool();
        }

        public delegate void DisconnectDelegate(string reason);

        public event DisconnectDelegate OnDisconnect;

        private void HandlePackage(Package message)
        {
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

            if (typeof(McpePlayerEquipment) == message.GetType())
            {
                OnMcpePlayerEquipment((McpePlayerEquipment)message);
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

            if (typeof(McpeRemovePlayer) == message.GetType())
            {
                OnRemovePlayer((McpeRemovePlayer)message);
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

            if (typeof(McpeTileEntityData) == message.GetType())
            {
                OnTileEntityData?.Invoke((McpeTileEntityData)message);
                return;
            }

            if (typeof(McpePlayerArmorEquipment) == message.GetType())
            {
                OnMcpePlayerArmor?.Invoke((McpePlayerArmorEquipment)message);
                return;
            }

            if (typeof(McpeRemoveEntity) == message.GetType())
            {
                OnMcpeRemoveEntity?.Invoke((McpeRemoveEntity)message);
                return;
            }

            if (typeof(McpePlayerStatus) == message.GetType())
            {
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

            if (typeof(McpeTileEvent) == message.GetType())
            {
                OnMcpeTileEvent?.Invoke((McpeTileEvent)message);
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

            Log.Warn($"Unhandled package: {message.GetType().Name}");
        }

        public delegate void PacketDelegate<in T>(T packet);

        public event PacketDelegate<McpePlayerList> OnMcpePlayerList;
        public event PacketDelegate<McpeAnimate> OnMcpeAnimate;
        public event PacketDelegate<McpeRespawn> OnMcpeRespawn;
        public event PacketDelegate<McpePlayerAction> OnPlayerAction;
        public event PacketDelegate<McpeTileEntityData> OnTileEntityData;
        public event PacketDelegate<McpePlayerArmorEquipment> OnMcpePlayerArmor;
        public event PacketDelegate<McpeRemoveEntity> OnMcpeRemoveEntity;
        public event PacketDelegate<McpePlayerStatus> OnMcpePlayerStatus;
        public event PacketDelegate<McpeAddItemEntity> OnMcpeAddItemEntity;
        public event PacketDelegate<McpeTakeItemEntity> OnMcpeTakeItemEntity;
        public event PacketDelegate<McpeChunkRadiusUpdate> OnMcpeChunkRadiusUpdate;
        public event PacketDelegate<McpeAdventureSettings> OnMcpeAdventureSettings;
        public event PacketDelegate<McpeEntityEvent> OnMcpeEntityEvent;
        public event PacketDelegate<McpeContainerOpen> OnMcpeContainerOpen;
        public event PacketDelegate<McpeTileEvent> OnMcpeTileEvent;
        public event PacketDelegate<McpeExplode> OnMcpeExplode;
        public event PacketDelegate<McpeMobEffect> OnMcpeMobEffect;

        public delegate void McpeRemovePlayerDelegate(long entityId, UUID clientUuid);

        public event McpeRemovePlayerDelegate OnPlayerRemoval;

        private void OnRemovePlayer(McpeRemovePlayer message)
        {
            OnPlayerRemoval?.Invoke(message.entityId, message.clientUuid);
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

        private void OnMcpePlayerEquipment(McpePlayerEquipment message)
        {
            OnPlayerEquipment?.Invoke(message.entityId, message.item);
        }

        public delegate void McpeContainerSetDataDelegate(byte windowId, short value, short property);

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

            OnSetSpawnPosition?.Invoke(new Vector3(msg.x, msg.y, msg.z));
        }

        private void OnConnectionRequestAccepted()
        {
            Thread.Sleep(50);
            SendNewIncomingConnection();
            Thread.Sleep(50);
            SendLogin(Username);
        }

        public delegate void FullChunkDataDelegate(ChunkColumn chunkColumn);
        public event FullChunkDataDelegate OnChunkData;

        private void OnFullChunkData(Package message)
        {
            McpeFullChunkData msg = (McpeFullChunkData)message;
            ChunkColumn chunk = new ChunkColumn
            {
                x = msg.chunkX,
                z = msg.chunkZ
            };

            using (var memStream = new MemoryStream(msg.chunkData))
            {
                using (NbtBinaryReader defStream = new NbtBinaryReader(memStream, true))
                {
                    int chunkSize = 16 * 16 * 128;
                    defStream.Read(chunk.blocks, 0, chunkSize);
                    defStream.Read(chunk.metadata.Data, 0, chunkSize / 2);
                    defStream.Read(chunk.skylight.Data, 0, chunkSize / 2);
                    defStream.Read(chunk.blocklight.Data, 0, chunkSize / 2);
                    defStream.Read(chunk.height, 0, 256);

                    byte[] ints = new byte[256 * 4];
                    defStream.Read(ints, 0, ints.Length);
                    int j = 0;
                    for (int i = 0; i < ints.Length; i = i + 4)
                    {
                        chunk.biomeColor[j++] = BitConverter.ToInt32(new[] { ints[i], ints[i + 1], ints[i + 2], ints[i + 3] }, 0);
                    }

                    defStream.ReadInt32(); //Extra Size

                    while (memStream.Position < memStream.Length)
                    {
                        try
                        {
                            NbtFile nbtFile = new NbtFile() { BigEndian = false };
                            nbtFile.LoadFromStream(memStream, NbtCompression.None);

                            var compound = nbtFile.RootTag;

                            NbtTag val;
                            if (compound.TryGet("id", out val))
                            {
                                if (val.StringValue == "Sign")
                                {
                                    Sign s = new Sign
                                    {
                                        Text1 = compound["Text1"].StringValue,
                                        Text2 = compound["Text2"].StringValue,
                                        Text3 = compound["Text3"].StringValue,
                                        Text4 = compound["Text4"].StringValue,
                                        Coordinates =
                                            new BlockCoordinates(compound["x"].IntValue, compound["y"].IntValue,
                                                compound["z"].IntValue)
                                    };

                                    chunk.BlockEntities.Add(s.Coordinates, s.GetCompound());
                                }
                                else if (val.StringValue == "Skull")
                                {
                                    SkullBlockEntity s = new SkullBlockEntity
                                    {
                                        SkullType = compound["SkullType"].ByteValue,
                                        Rotation = compound["Rot"].ByteValue,
                                        Coordinates =
                                            new BlockCoordinates(compound["x"].IntValue, compound["y"].IntValue,
                                                compound["z"].IntValue)
                                    };

                                    chunk.BlockEntities.Add(s.Coordinates, s.GetCompound());
                                }
                            }
                        }
                        catch
                        {
                            //
                        }
                    }
                }
            }

            OnChunkData?.Invoke(chunk);
        }

        public event PacketDelegate<McpeSetEntityMotion> OnMcpeSetEntityMotion;
        private void HandleMcpeSetEntityMotion(McpeSetEntityMotion packet)
        {
            var count = packet.ReadInt();
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

            OnMcpeSetEntityMotion?.Invoke(packet);
        }

        public event PacketDelegate<McpeMoveEntity> OnMcpeMoveEntity;
        private void HandleMcpeMoveEntity(McpeMoveEntity packet)
        {
            EntityLocations el = new EntityLocations();

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
            OnMcpeMoveEntity?.Invoke(packet);
        }

        private void OnBatch(Package message)
        {
            McpeBatch batch = (McpeBatch)message;

            var messages = new List<Package>();

            // Get bytes
            byte[] payload = batch.payload;
            // Decompress bytes

            using (MemoryStream stream = new MemoryStream(payload))
            {
                if (stream.ReadByte() != 0x78)
                {
                    throw new InvalidDataException("Incorrect ZLib header. Expected 0x78 0x9C");
                }
                stream.ReadByte();
                using (var defStream2 = new DeflateStream(stream, CompressionMode.Decompress, false))
                {
                    // Get actual package out of bytes
                    using (MemoryStream destination = new MemoryStream())
                    {
                        defStream2.CopyTo(destination);
                        destination.Position = 0;
                        using (NbtBinaryReader reader = new NbtBinaryReader(destination, true))
                        {
                            do
                            {
                                int len = reader.ReadInt32();
                                byte[] internalBuffer = reader.ReadBytes(len);

                                if (internalBuffer[0] == 0x8e) Log.Error("Wrong code");

                                var package = PackageFactory.CreatePackage(internalBuffer[0], internalBuffer) ??
                                              new UnknownPackage(internalBuffer[0], internalBuffer);
                                messages.Add(package);

                                //Log.Debug($"Batch: {package.GetType().Name} 0x{package.Id:x2}");

                            } while (destination.Position < destination.Length);
                        }
                    }
                }
            }

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
                SendDatagram(datagram);
            }
        }

        private void SendDatagram(Datagram datagram)
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
            try
            {
                //UdpClient.SendAsync(data, data.Length, targetEndpoint);
                UdpClient.Send(data, data.Length, targetEndpoint);
            }
            catch (Exception e)
            {
                Log.Debug("Send exception", e);
            }
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
                raknetProtocolVersion = 6,
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
            _clientGuid = new Random().Next();
            var packet = new OpenConnectionRequest2
            {
                clientendpoint = _clientEndpoint,
                mtuSize = _mtuSize,
                clientGuid = _clientGuid
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
            Random rand = new Random();
            var packet = new NewIncomingConnection
            {
                doSecurity = 163,
                session = rand.Next(),
                session2 = rand.Next(),
                cookie = rand.Next(),
                port = (short)_clientEndpoint.Port
            };

            SendPackage(packet);
        }

        private void SendLogin(string username)
        {
            Skin skin = new Skin
            {
                Slim = false,
                Texture = Encoding.Default.GetBytes(new string('Z', 8192)),
                SkinType = "Standard_Custom"
            };

            if (File.Exists("skin.png"))
            {
                skin.Texture = File.ReadAllBytes("skin.png");
            }

            var packet = new McpeLogin
            {
                username = username,
                protocol = 46,
                protocol2 = 46,
                clientId = ClientId,
                clientUuid = new UUID(Guid.NewGuid().ToByteArray()),
                serverAddress = _serverEndpoint.Address + ":" + _serverEndpoint.Port,
                // clientSecret = "iwmvi45hm85oncyo58",
                clientSecret =
                    Encoding.ASCII.GetString(
                        MD5.Create()
                            .ComputeHash(
                                Encoding.UTF8.GetBytes("" + ClientId + _serverEndpoint.Address + _serverEndpoint.Port))),
                skin = skin
            };

            byte[] buffer = Player.CompressBytes(packet.Encode(), CompressionLevel.Fastest, true);

            McpeBatch batch = new McpeBatch
            {
                payloadSize = buffer.Length,
                payload = buffer
            };
            batch.Encode();

            SendPackage(batch);
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
            packet.x = 0;
            packet.y = 0;
            packet.z = 0;
            SendPackage(packet);
        }
    }
}