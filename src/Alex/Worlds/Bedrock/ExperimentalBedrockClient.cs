using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Alex.API.Data;
using Alex.API.Entities;
using Alex.API.Events;
using Alex.API.Network.Bedrock;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Services;
using Ionic.Zlib;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Utils.Skins;
using MojangSharp.Endpoints;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using SharpRakLib.Core;
using SharpRakLib.Core.Client;
using SharpRakLib.Protocol.RakNet;
using SharpRakLib.Server;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using Compression = SharpRakLib.Core.Compression;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using LevelInfo = MiNET.Worlds.LevelInfo;
using NewtonsoftMapper = MiNET.NewtonsoftMapper;
using PlayerLocation = MiNET.Utils.PlayerLocation;
using VarInt = SharpRakLib.Core.VarInt;

namespace Alex.Worlds.Bedrock
{
    public class ExperimentalBedrockClient : IBedrockNetworkProvider, IChatProvider, IDisposable, HookManager.IHookRunnable
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();
        
        private McpeClientMessageDispatcher MessageDispatcher { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        
        private BedrockWorldProvider WorldProvider { get; }
        
        private RakNetClient Client { get; }
        private ClientSession ClientSession { get; set; }
        private PlayerProfile Profile { get; }
        private IPEndPoint Endpoint { get; }
        
        public ExperimentalBedrockClient(Alex alex, IServiceProvider serviceProvider, BedrockWorldProvider worldProvider, IPEndPoint endPoint)
        {
            CancellationTokenSource = new CancellationTokenSource();
            WorldProvider = worldProvider;
            
            var profileService = serviceProvider.GetService<IPlayerProfileService>();
            Profile = profileService.CurrentProfile;
            
            MessageDispatcher = new McpeClientMessageDispatcher(new BedrockClientPacketHandler(this, serviceProvider.GetRequiredService<IEventDispatcher>(), worldProvider, Profile, alex, CancellationTokenSource.Token));
           //MessageDispatcher = new McpeClientMessageDispatcher(new ExperimentalClientMessageHandler(this));

            XblmsaService = serviceProvider.GetService<XBLMSAService>();

            Endpoint = endPoint;
            Client = new ThreadedRaknetClient(endPoint);
            Client.HookManager.AddHook(HookManager.Hook.PacketRecieved, this);
        }

        private XBLMSAService XblmsaService { get; }

        public bool IsConnected
        {
            get { return ClientSession.IsConnected; }
        }

        public void OnHook(SessionBase session, params object[] param)
        {
            if (session is ClientSession clientSession)
            {
                ClientSession = clientSession;
                EncapsulatedPacket pk = (EncapsulatedPacket) param[0];
                var id = pk.Payload[0];

                var packet = PacketFactory.Create(id, pk.Payload, "raknet");
                //Log.Info($"Received: {packet}");
                HandlePacket(packet);
            }
        }
        
        private void HandlePacket(Packet message)
        {
            if (message == null)
            {
                Log.Warn($"RakNet = null");
                return;
            }

            if (message is ConnectionRequestAccepted requestAccepted)
            {
                HandleConnectionRequestAccepted(requestAccepted);
            }
            else if (message is McpeWrapper wrapper)
            {
                HandleMcpeWrapper(wrapper);
            }
            else if (!MessageDispatcher.HandlePacket(message))
            {
                Log.Warn(
                    $"Unhandled packet 0x{message.Id:X2} {message.GetType().Name}\n{Packet.HexDump(message.Bytes)}");
            }
        }
        
        private void HandleConnectionRequestAccepted(ConnectionRequestAccepted message)
        {
            ConnectionAcceptedWaitHandle?.Set();
            
            var packet = NewIncomingConnection.CreateObject();
            packet.clientendpoint = Endpoint;
            packet.systemAddresses = new IPEndPoint[20];
            for (int i = 0; i < 20; i++)
            {
                packet.systemAddresses[i] = new IPEndPoint(IPAddress.Any, 0);
            }
                
            
            PlayerStatus = ClientSession.Connected;
            SendPacket(packet, false);

            Thread.Sleep(2500);
            
            SendLogin();
        }

        private void HandleMcpeWrapper(McpeWrapper batch)
        {
            var messages = new List<Packet>();

            // Get bytes
            byte[] payload = batch.payload;
            MemoryStream stream = new MemoryStream(payload);
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
                    do
                    {
                        byte[] internalBuffer = null;
                        try
                        {
                            int len = (int) VarInt.ReadUInt32(destination);
                            long pos = destination.Position;
                            int id = (int) VarInt.ReadUInt32(destination);
                            len = (int) (len - (destination.Position -
                                                pos)); // calculate len of buffer after varint
                            internalBuffer = new byte[len];
                            destination.Read(internalBuffer, 0, len);

                            if (id == 0x8e)
                                throw new Exception("Wrong code, didn't expect a 0x8E in a batched packet");

                            var packet = PacketFactory.Create((byte) id, internalBuffer, "mcpe") ??
                                         new UnknownPacket((byte) id, internalBuffer);
                            messages.Add(packet);

                            //if (Log.IsDebugEnabled) Log.Debug($"Batch: {packet.GetType().Name} 0x{packet.Id:x2}");
                            if (packet is UnknownPacket)
                                Log.Error($"Batch: {packet.GetType().Name} 0x{packet.Id:x2}");
                            //if (!(package is McpeFullChunkData)) Log.Debug($"Batch: {package.GetType().Name} 0x{package.Id:x2} \n{Package.HexDump(internalBuffer)}");
                        }
                        catch (Exception e)
                        {
                            if (internalBuffer != null)
                                Log.Error($"Batch error while reading:\n{Packet.HexDump(internalBuffer)}");
                            Log.Error(e,"Batch processing");
                        }
                    } while (destination.Position < destination.Length);
                }
            }

            //Log.Error($"Batch had {messages.Count} packets.");
            if (messages.Count == 0) Log.Error($"Batch had 0 packets.");

            foreach (var msg in messages)
            {
                msg.DatagramSequenceNumber = batch.DatagramSequenceNumber;
                msg.OrderingChannel = batch.OrderingChannel;
                msg.OrderingIndex = batch.OrderingIndex;
                HandlePacket(msg);
                msg.PutPool();
            }
        }
        
        public void SendLogin()
        {
            JWT.JsonMapper = new NewtonsoftMapper();

            var clientKey = XblmsaService.MinecraftKeyPair;// CryptoUtils.GenerateClientKey();

            ECDsa signKey = ConvertToSingKeyFormat(clientKey);

            string b64Key = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(clientKey.Public).GetEncoded().EncodeBase64();

            string identity, xuid = "";
			byte[] certChain = null;

            string username = Profile.Username;
            if (XblmsaService.MinecraftChain != null)
            {
	            var element = XblmsaService.DecodedChain.Chain[1];

                username = element.ExtraData.DisplayName;
                identity = element.ExtraData.Identity;
                xuid = element.ExtraData.Xuid;
                
                certChain = XblmsaService.MinecraftChain;
            }
            else
            {
                long iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long exp = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

                //ECDsa signKey = ConvertToSingKeyFormat(newKey);

                CertificateData certificateData = new CertificateData
                {
                    Exp = exp,
                    Iat = iat,
                    ExtraData = new ExtraData
                    {
                        DisplayName = username,
                        Identity = Guid.NewGuid().ToString(),
                        XUID = ""
                    },
                    Iss = "self",
                    IdentityPublicKey = b64Key,
                    CertificateAuthority = true,
                    Nbf = iat,
                    RandomNonce = new Random().Next(),
                };

                certChain = EncodeJwt(certificateData, b64Key, signKey);
            }

            var skinData = EncodeSkinJwt(signKey, username, b64Key);

	        byte[] data = CryptoUtils.CompressJwtBytes(certChain, skinData, CompressionLevel.Fastest);

            McpeLogin loginPacket = new McpeLogin
            {
                protocolVersion = McpeProtocolInfo.ProtocolVersion,
                payload = data
            };

            /*Session.CryptoContext = new CryptoContext()
            {
	            ClientKey = clientKey,
	            UseEncryption = false,
            };*/

            SendPacket(loginPacket);
        }
        
          private byte[] EncodeJwt(CertificateData certificateData, string b64Key, ECDsa signKey)
        {
	        string val = JWT.Encode(certificateData, signKey, JwsAlgorithm.ES384, new Dictionary<string, object> { { "x5u", b64Key } }, new JwtSettings()
            {
                JsonMapper = new JWTMapper()
            });

           // Log.Warn(JWT.Payload(val));

          //  Log.Warn(string.Join(";", JWT.Headers(val)));

            val = $@"{{ ""chain"": [""{val}""] }}";

            return Encoding.UTF8.GetBytes(val);
        }

        private byte[] EncodeSkinJwt(ECDsa signKey, string username, string x5u)
        {
            MiNET.Utils.Skins.Skin skin = new MiNET.Utils.Skins.Skin 
            {
                Slim = false,
                Data = Encoding.Default.GetBytes(new string('Z', 8192)),
                SkinId = "Standard_Custom",
                //  GeometryName = "geometry.humanoid.custom",
                //  GeometryData = "",
                //   Cape = new Cape()
                // {
                //      Data = new byte[0]
                //  },
                //  SkinGeometryName = "geometry.humanoid.custom",
                // SkinGeometry = ""
            };

            string skin64 = Convert.ToBase64String(skin.Data);
            // string cape64 = Convert.ToBase64String(skin.Cape.Data);

            string skinData = $@"
{{
    ""SkinAnimationData"": null,
    ""AnimatedImageData"": [],
	""CapeData"": """",
	""ADRole"": 0,
	""ClientRandomId"": {new Random().Next()},
	""CurrentInputMode"": 1,
	""DefaultInputMode"": 1,
	""DeviceModel"": ""Alex"",
	""DeviceOS"": 7,
	""GameVersion"": ""{McpeProtocolInfo.GameVersion}"",
	""IsEduMode"": {Config.GetProperty("EnableEdu", false).ToString().ToLower()},
	""GuiScale"": 0,
	""LanguageCode"": ""en_US"",
	""PlatformOfflineId"": """",
	""PlatformOnlineId"": """",
	""SelfSignedId"": ""{Guid.NewGuid().ToString()}"",
	""ServerAddress"": ""{Endpoint.Address.ToString()}:{Endpoint.Port.ToString()}"",
	""SkinData"": ""{skin64}"",
	""SkinImageWidth"": 32,
	""SkinImageHeight"": 64,
	""PremiumSkin"": false,
    ""PersonaSkin"": false,
	""CapeImageHeight"": 0,
	""CapeImageWidth"": 0,
    ""CapeOnClassicSkin"": false,
	""SkinId"": ""{skin.SkinId}"",
	""TenantId"": ""38dd6634-1031-4c50-a9b4-d16cd9d97d57"",
	""ThirdPartyName"": ""{username}"",
	""UIProfile"": 0,
	""IsAlex"": 1
}}";

            string val = JWT.Encode(skinData, signKey, JwsAlgorithm.ES384, new Dictionary<string, object> { { "x5u", x5u } }, new JwtSettings()
            {
                JsonMapper = new JWTMapper()
            });

            return Encoding.UTF8.GetBytes(val);
        }
        
        private static ECDsa ConvertToSingKeyFormat(AsymmetricCipherKeyPair key)
        {
            ECPublicKeyParameters pubAsyKey = (ECPublicKeyParameters)key.Public;
            ECPrivateKeyParameters privAsyKey = (ECPrivateKeyParameters)key.Private;

            var signParam = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP384,
                Q =
                {
                    X = pubAsyKey.Q.AffineXCoord.GetEncoded(),
                    Y = pubAsyKey.Q.AffineYCoord.GetEncoded()
                }
            };
            signParam.D = CryptoUtils.FixDSize(privAsyKey.D.ToByteArrayUnsigned(), signParam.Q.X.Length);
            signParam.Validate();

            return ECDsa.Create(signParam);
        }

        public void EntityAction(int entityId, EntityAction action)
        {
            throw new NotImplementedException();
        }

        public void SendChatMessage(string message)
        {
            throw new NotImplementedException();
        }

        public void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
        {
            throw new NotImplementedException();
        }

        public void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
        {
            throw new NotImplementedException();
        }

        public void EntityInteraction(IEntity player, IEntity target, McpeInventoryTransaction.ItemUseOnEntityAction action)
        {
            throw new NotImplementedException();
        }

        public void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
        {
            throw new NotImplementedException();
        }

        public void UseItem(int hand)
        {
            throw new NotImplementedException();
        }

        public void HeldItemChanged(short slot)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IWorldReceiver WorldReceiver { get; set; }
        public System.Numerics.Vector3 SpawnPoint { get; set; } = System.Numerics.Vector3.Zero;
        public LevelInfo LevelInfo { get; } = new LevelInfo();
        public PlayerLocation CurrentLocation { get; set; } = new PlayerLocation();
        public int ChunkRadius { get; set; } = 6;
        public long EntityId { get; set; }
        public long NetworkEntityId { get; set; }

        public int PlayerStatus
        {
            get
            {
                return ClientSession._state;
            }
            set
            {
                ClientSession._state = value;
                if (value == 3)
                {
                    PlayerStatusChanged?.Set();
                }
            }
        }

        public bool HasSpawned { get; set; }
        public AutoResetEvent PlayerStatusChanged { get; set; } = new AutoResetEvent(false);
        public IChatReceiver ChatReceiver { get; }
        
        public void SendPacket(Packet packet, bool immediate)
        {
            if (PlayerStatus == ClientSession.Connected && !(packet is McpeWrapper))
            {
                McpeWrapper wrapperPacket = new McpeWrapper();
                wrapperPacket.payload = Compression.Compress(packet.Encode(), true);
                packet = wrapperPacket;
            }
               
            EncapsulatedPacket encapsulatedPacket = new EncapsulatedPacket();
            encapsulatedPacket.Payload = packet.Encode();
                
            ClientSession.AddPacketToQueue(encapsulatedPacket, immediate);
        }

        private ManualResetEventSlim ConnectionAcceptedWaitHandle { get; set; }
        public void Start(ManualResetEventSlim resetEvent)
        {
            ConnectionAcceptedWaitHandle = resetEvent;
            
            Client.Start();

            ClientSession = Client.WaitForSession();
        }

        public void SendPacket(Packet packet)
        {
            this.SendPacket(packet, true);
        }
        
        public void RequestChunkRadius(int radius)
        {
            throw new NotImplementedException();
        }

        public void SendDisconnectionNotification()
        {
            throw new NotImplementedException();
        }

        public void SendMcpeMovePlayer()
        {
            throw new NotImplementedException();
        }

        public void InitiateEncryption(byte[] x5u, byte[] salt)
        {
            throw new NotImplementedException();
        }

        public void ShowDisconnect(string reason, bool useTranslation = false)
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public void RequestTabComplete(string text, out int transactionId)
        {
            throw new NotImplementedException();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}