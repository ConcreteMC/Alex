using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API;
using Alex.API.Data.Options;

using Alex.API.Services;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Net.Bedrock.Packets;
using Alex.Net.Bedrock.Raknet;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Utils.Inventories;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Net;
using MiNET.Net.RakNet;
using MiNET.Plugins;
using MiNET.Utils;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using BlockFace = Alex.API.Blocks.BlockFace;
using ConnectionInfo = Alex.API.Network.ConnectionInfo;
using CryptoContext = Alex.Net.Bedrock.CryptoContext;
using DedicatedThreadPool = MiNET.Utils.DedicatedThreadPool;
using Description = MiNET.Utils.Skins.Description;
using Item = Alex.Items.Item;
using LevelInfo = MiNET.Worlds.LevelInfo;
using Player = Alex.Entities.Player;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockClient : NetworkProvider, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClient));
		
		private ManualResetEventSlim ConnectionAcceptedWaitHandle { get; set; }
		public BedrockWorldProvider WorldProvider { get; }
		public EventHandler<BedrockMotd> OnMotdReceivedHandler;
		//public BedrockMotd KnownMotd = new BedrockMotd(string.Empty);

        private Alex                Alex                { get; }
        public  ResourcePackManager ResourcePackManager { get; set; }
        private IOptionsProvider    OptionsProvider     { get; }
        private XboxAuthService     XboxAuthService     { get; }
        private AlexOptions         Options             => OptionsProvider.AlexOptions;
        
        private CancellationTokenSource CancellationTokenSource { get; }
        
        public McpeNetworkChunkPublisherUpdate LastChunkPublish { get; set; }
        public bool CanSpawn => PlayerStatus == 3;

        public AutoResetEvent ChangeDimensionResetEvent { get; } = new AutoResetEvent(false);
        public RaknetConnection Connection { get; }
        private MessageHandler MessageHandler { get; set; }
        private RaknetSession Session { get; set; }//=> Connection.ConnectionInfo.RakSessions.Values.FirstOrDefault();
        public override bool IsConnected => Session?.State == ConnectionState.Connected;
        
        private IPEndPoint _remoteEndpoint;

        public IPEndPoint ServerEndpoint
        {
	        get { return _remoteEndpoint; }
	        set
	        {
		        Connection.RemoteEndpoint = value;
		        _remoteEndpoint = value;
	        }
        }

        public  TimeSpan                   TimeSinceLastPacket => MessageHandler?.TimeSinceLastPacket ?? TimeSpan.Zero;
        public  bool                       GameStarted         { get; set; } = false;
        private ChunkProcessor             ChunkProcessor      { get; }
        private BedrockClientPacketHandler PacketHandler       { get; set; }

        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        public BedrockClient(Alex alex, IPEndPoint endpoint, PlayerProfile playerProfile, BedrockWorldProvider wp)
		{
			PacketFactory.CustomPacketFactory = new AlexPacketFactory();
			CancellationTokenSource = new CancellationTokenSource();
			
            Alex = alex;
			WorldProvider = wp;
			OptionsProvider = alex.Services.GetRequiredService<IOptionsProvider>();
			XboxAuthService = alex.Services.GetRequiredService<XboxAuthService>();
			
			ChunkRadius = Options.VideoOptions.RenderDistance;
			
			_disposables.Add(Options.VideoOptions.RenderDistance.Bind(RenderDistanceChanged));
			_disposables.Add(Options.VideoOptions.ClientSideLighting.Bind(ClientSideLightingChanged));
		//WorkerThreadPool = threadPool;
			//ReflectionHelper.SetPrivateStaticFieldValue();
			//MiNetServer.FastThreadPool = threadPool;

			_disposables.Add(ResourcePackManager = new ResourcePackManager(this));

			if (wp != null)
			{
				ChunkProcessor = new ChunkProcessor(
					this,
					alex.Services.GetRequiredService<IOptionsProvider>().AlexOptions.MiscelaneousOptions
					   .ServerSideLighting, CancellationTokenSource.Token,
					Alex.Services.GetRequiredService<BlobCache>());

				ChunkProcessor.ClientSideLighting = Options.VideoOptions.ClientSideLighting;
				//ChunkProcessor.Instance = ChunkProcessor;
			}

			Connection = new RaknetConnection();
			ServerEndpoint = endpoint;

			Connection.CustomMessageHandlerFactory = session =>
			{
				//Log.Info($"Requesting sessions...  {Connection.ConnectionInfo.RakSessions.Count}");

			//	bool hasSession = Session != null;

			BedrockClientPacketHandler packetHandler;
			var handler = new MessageHandler(session, packetHandler = new BedrockClientPacketHandler(this, wp, playerProfile, alex, CancellationTokenSource.Token, ChunkProcessor));
				
				if (MessageHandler != null)
					Log.Warn($"Messagehandler was already set.");
				
				if (Session == null)
				{
					Session = session;
					
					handler.ConnectionAction = () =>
					{
						ConnectionAcceptedWaitHandle?.Set();
						SendAlexLogin(playerProfile.Username);
					};

					handler.DisconnectedAction = (reason, sendDisconnect) =>
					{
						Log.Warn($"Got disconnected from server: {reason}");
						ShowDisconnect(reason, false, false, DisconnectReason.Unknown);
					};
					
					MessageHandler = handler;
					PacketHandler = packetHandler;
				}

				return handler;
			};
		}

		private void ClientSideLightingChanged(bool oldvalue, bool newvalue)
		{
			ChunkProcessor.ClientSideLighting = newvalue;
		}

		private bool     Starting    { get; set; } = false;
		private DateTime StartTime   { get; set; }
		private Timer    ThroughPut { get; set; }

		public bool Start(TimeSpan timeout)
		{
			if (Starting)
				return false;

			Starting = true;

			StartTime = DateTime.UtcNow;
			var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(CancellationTokenSource.Token);
			//	var player = WorldReceiver.Player;

			//player.Inventory.CursorChanged += InventoryOnCursorChanged;
			//	player.Inventory.SlotChanged += InventoryOnSlotChanged;

			//ConnectionAcceptedWaitHandle = resetEvent;

			//return Task.Run((Func<bool>)(() =>
			{
				if (!Connection.AutoConnect)
				{
					//Connection.ConnectionInfo.ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);

					if (TryLocate(ServerEndpoint, out var serverInfo, 3))
					{
						OnMotdReceivedHandler?.Invoke(
							this,
							new BedrockMotd(serverInfo.serverName)
							{
								ServerEndpoint = serverInfo.serverEndPoint, Latency = serverInfo.ping
							});
						//resetEvent.Set();

						return true;
					}
				}
				else
				{
					Connection.Start();
					
					cancellationToken.CancelAfter(timeout);
					if (Connection.TryConnect(ServerEndpoint, cancellationToken: cancellationToken.Token))
					{
						ThroughPut = new Timer(state => { UpdateConnectionInfo();}, null, 1000, 1000);

						//resetEvent.Set();
						return true;
					}
				}

				return false;
			} //));
		}

		private System.Timers.Timer _timer;

		private void UpdateConnectionInfo()
		{
			long packetSizeOut = Interlocked.Exchange(ref Connection.ConnectionInfo.BytesOut, 0L);
			long packetSizeIn  = Interlocked.Exchange(ref Connection.ConnectionInfo.BytesIn, 0L);

			double throughtPutOut = (double) (packetSizeOut) / 1000.0;
			double throughPutIn   = (double) (packetSizeIn) / 1000.0;

			long packetCountOut = Interlocked.Exchange(ref Connection.ConnectionInfo.PacketsOut, 0L);
			long packetCountIn  = Interlocked.Exchange(ref Connection.ConnectionInfo.PacketsIn, 0L);

			long ackReceived = Interlocked.Exchange(ref Connection.ConnectionInfo.Ack, 0L);
			long ackSent     = Interlocked.Exchange(ref Connection.ConnectionInfo.AckSent, 0L);
			long nakReceive  = Interlocked.Exchange(ref Connection.ConnectionInfo.Nak, 0L);
			var  nakSent     = Interlocked.Exchange(ref Connection.ConnectionInfo.NakSent, 0);

			long resends = Interlocked.Exchange(ref Connection.ConnectionInfo.Resends, 0L);
			long fails   = Interlocked.Exchange(ref Connection.ConnectionInfo.Fails, 0L);

			string str =
				$"Pkt in/out(#/s) {packetCountIn}/{packetCountOut}, ACK in/out(#/s) {ackReceived}/{ackSent}, NAK in/out(#/s) {nakReceive}/{nakSent}, THR in/out(Kbps){throughPutIn:F2}/{throughtPutOut:F2}";

			//if (Config.GetProperty("ServerInfoInTitle", false))
			//	Console.Title = str;
			//else
			Log.Info(str);

			ConnectionInfo.NetworkState networkState = ConnectionInfo.NetworkState.Ok;

			if (nakSent > 0)
			{
				networkState = ConnectionInfo.NetworkState.PacketLoss;
			}
			else if (Connection.IsNetworkOutOfOrder)
			{
				networkState = ConnectionInfo.NetworkState.OutOfOrder;
			}
			else if (MessageHandler != null && MessageHandler.TimeSinceLastPacket.TotalMilliseconds >= 250)
			{
				networkState = ConnectionInfo.NetworkState.Slow;
			}
			else if (Connection.ConnectionInfo.Latency > 250)
			{
				networkState = ConnectionInfo.NetworkState.HighPing;
			}

			_connectionInfo = new ConnectionInfo(
				StartTime, Connection.ConnectionInfo.Latency, nakReceive, ackReceived, ackSent, fails, resends,
				packetSizeIn, packetSizeOut, packetCountIn, packetCountOut, networkState) {NakSent = nakSent};
		}

		public bool TryLocate(
			IPEndPoint targetEndPoint,
			out (IPEndPoint serverEndPoint, string serverName, long ping) serverInfo,
			int numberOfAttempts = 2147483647)
		{
			Stopwatch sw = new Stopwatch();
			this.Connection.Start();

			
			while (!this.Connection.FoundServer)
			{
				if ((!sw.IsRunning || sw.ElapsedMilliseconds > 100) && numberOfAttempts-- > 0)
				{
					sw.Restart();
					this.SendUnconnectedPingInternal(targetEndPoint);
				}

				if (numberOfAttempts <= 0)
					break;

				//Task.Delay(100).Wait();
			}

			sw.Stop();
			
			serverInfo = (this.Connection.RemoteEndpoint, this.Connection.RemoteServerName, sw.ElapsedMilliseconds);
			
			return this.Connection.FoundServer;
		}
		
		private void SendUnconnectedPingInternal(IPEndPoint targetEndPoint)
		{
			byte[] data = new UnconnectedPing()
			{
				pingId = Stopwatch.GetTimestamp(),
				guid = this.Connection.ClientGuid
			}.Encode();
			
			if (targetEndPoint != null)
				this.Connection.SendData(data, targetEndPoint);
			else
				this.Connection.SendData(data, new IPEndPoint(IPAddress.Broadcast, 19132));
		}

		private ConnectionInfo _connectionInfo = new ConnectionInfo(DateTime.UtcNow, 0, 0,0,0,0,0,0,0, 0, 0);
		public override ConnectionInfo GetConnectionInfo()
		{
			return _connectionInfo;
		}

		/// <inheritdoc />
		public override void PlayerOnGroundChanged(Player player, bool onGround)
		{
			
		}

		/// <inheritdoc />
		public override void EntityFell(long entityId, float distance, bool inVoid)
		{
			/*McpeEntityFall fall = McpeEntityFall.CreateObject();
			fall.runtimeEntityId = entityId;
			fall.fallDistance = distance;
			fall.isInVoid = inVoid;
			SendPacket(fall);*/
		}

		private void InventoryOnSlotChanged(object? sender, SlotChangedEventArgs e)
		{
			if (e.IsServerTransaction)
				return;
			
			int inventoryId = 0;
			int slot = e.Index;
			if (e.Index >= 36 && e.Index <= 39)
			{
				inventoryId = 120;
				slot = 39 - e.Index;
			}
			
			ContainerTransactionRecord transactionRecord = new ContainerTransactionRecord();
			transactionRecord.InventoryId = inventoryId;
			transactionRecord.Slot = slot;
			transactionRecord.NewItem = GetMiNETItem(e.Value);
			transactionRecord.OldItem = GetMiNETItem(e.OldItem);

			var packet = McpeInventoryTransaction.CreateObject();
			packet.transaction = new NormalTransaction()
			{
				TransactionRecords = new List<TransactionRecord>(){
					transactionRecord
				}
			};
			
			SendPacket(packet);
		}

		private void InventoryOnCursorChanged(object? sender, SlotChangedEventArgs e)
		{
			if (e.IsServerTransaction)
				return;
			
			ContainerTransactionRecord transactionRecord = new ContainerTransactionRecord();
			transactionRecord.InventoryId = 124;
			transactionRecord.Slot = 0;
			transactionRecord.NewItem = GetMiNETItem(e.Value);
			transactionRecord.OldItem = GetMiNETItem(e.OldItem);

			var packet = McpeInventoryTransaction.CreateObject();
			packet.transaction = new NormalTransaction()
			{
				TransactionRecords = new List<TransactionRecord>(){
					transactionRecord
				}
			};
			
			SendPacket(packet);
		}

		public void SendPacket(Packet packet)
		{
			if (Session == null)
			{
				return;
			}
		//	Log.Info($"Sent: {packet}");
			Session.SendPacket(packet);
		}

		private bool _markedAsInitialized = false;
		public void MarkAsInitialized()
		{
			if (_markedAsInitialized)
			{
				Log.Warn($"Tried sending initialize more than once");

				return;
			}
			
			Log.Info($"Sent LocalPlayerInitialized");
			var packet = McpeSetLocalPlayerAsInitialized.CreateObject();
			packet.runtimeEntityId = EntityId;
			packet.ReliabilityHeader.Reliability = Reliability.Reliable;
			
			//Connection.Session.SendDirectPacket(packet);
			SendPacket(packet);

			_markedAsInitialized = true;

			//_isInitialized = true;
		}

		public int ChunkRadius { get; set; }
		public long EntityId { get; set; }
		public long NetworkEntityId { get; set; }
		public int PlayerStatus { get; set; }

		private void RenderDistanceChanged(int oldvalue, int newvalue)
		{
			ChunkRadius = newvalue;
			//base.ChunkRadius = newvalue;
			RequestChunkRadius(newvalue);
		}

		public  bool             Transfered       { get; set; } = false;
		public  DisconnectReason DisconnectReason { get; private set; }
		private bool             _disconnectShown = false;
		public void ShowDisconnect(string reason, bool useTranslation = false, bool overrideActive = false, DisconnectReason disconnectReason = DisconnectReason.Unknown)
		{
			if (Transfered)
				return;
		
			if (_disconnectShown && overrideActive && Alex.GameStateManager.GetActiveState() is DisconnectedScreen s)
			{
				if (useTranslation)
				{
					s.DisconnectedTextElement.TranslationKey = reason;
				}
				else
				{
					s.DisconnectedTextElement.Text = reason;
				}

				return;
			}

			if (_disconnectShown)
				return;
			
			_disconnectShown = true;

			DisconnectReason = disconnectReason;

			s = new DisconnectedScreen();
            if (useTranslation)
            {
                s.DisconnectedTextElement.TranslationKey = reason;
            }
            else
            {
                s.DisconnectedTextElement.Text = reason;
            }

            Alex.GameStateManager.SetActiveState(s, false);
            Alex.GameStateManager.RemoveState("play");
            Dispose();
        }

		/*public override void OnConnectionRequestAccepted()
		{
			ConnectionAcceptedWaitHandle.Set();

			SendNewIncomingConnection();
			//_connectedPingTimer = new Timer(state => SendConnectedPing(), null, 1000, 1000);

			SendAlexLogin(Username);
		}
*/
		private bool LoginSent { get; set; } = false;
		private void SendAlexLogin(string username)
        {
	        if (LoginSent)
	        {
		        Log.Warn($"Tried to sent login again!");
		        return;
	        }

	        LoginSent = true;
            JWT.JsonMapper = new JWTMapper();

            var clientKey = XboxAuthService.MinecraftKeyPair;// CryptoUtils.GenerateClientKey();

            ECDsa signKey = ConvertToSingKeyFormat(clientKey);

            string b64Key = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(clientKey.Public).GetEncoded().EncodeBase64();

            string identity, xuid = "";
			byte[] certChain = null;

			//MinecraftChain = What i get back from the XBOXLive auth
            if (XboxAuthService.MinecraftChain != null)
            {
	            string raw = Encoding.UTF8.GetString(XboxAuthService.MinecraftChain);
	            dynamic a     = JObject.Parse(raw);
	            var     chain = ((JArray)a.chain).Values<string>().ToArray();
	            IDictionary<string, dynamic> chainHeader = JWT.Headers(chain[0]);
	            string x5u = chainHeader["x5u"];

	            var element = XboxAuthService.DecodedChain.Chain[1];

                username = element.ExtraData.DisplayName;
                identity = element.ExtraData.Identity;
                xuid = element.ExtraData.Xuid;

                long iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                long exp = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

                string val = JWT.Encode(new
                {
	                certificateAuthority=true,
	                exp= exp,
	                identityPublicKey= x5u,
	                nbf= iat
                }, signKey, JwsAlgorithm.ES384, new Dictionary<string, object> { { "x5u", b64Key } }, new JwtSettings()
                {
	                JsonMapper = new JWTMapper()
                });
                
                chain = new string[]
                {
	                val,
	                chain[0],
	                chain[1]
                };

                var hack = new
                {
	                chain= chain
                };

                var jsonChain = JsonConvert.SerializeObject(hack);

                certChain = Encoding.UTF8.GetBytes(jsonChain); // XblmsaService.MinecraftChain;
                //File.WriteAllBytes("/home/kenny/xbox.json", certChain);
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

                certChain = EncodeJwt(certificateData, b64Key, signKey, true);
            }

            var skinData = EncodeSkinJwt(clientKey, signKey, username, b64Key);

	        byte[] data = CryptoUtils.CompressJwtBytes(certChain, skinData, CompressionLevel.Fastest);

            McpeLogin loginPacket = new McpeLogin
            {
                protocolVersion = McpeProtocolInfo.ProtocolVersion,
                payload = data
            };

            MessageHandler.CryptoContext = new MiNET.Utils.CryptoContext() {ClientKey = clientKey, UseEncryption = false,};

            SendPacket(loginPacket);

            /*     var packet = McpeClientCacheStatus.CreateObject();
                 packet.enabled = false;
				     
                 Session.SendPacket(packet);*/

        //    Session.CryptoContext.UseEncryption = true;
        }

		public void SendMcpeMovePlayer(MiNET.Utils.PlayerLocation location, bool onGround)
		{
			var movePlayerPacket = McpeMovePlayer.CreateObject();
			movePlayerPacket.runtimeEntityId = EntityId;
			movePlayerPacket.otherRuntimeEntityId = NetworkEntityId;
			movePlayerPacket.x = location.X;
			movePlayerPacket.y = location.Y;
			movePlayerPacket.z = location.Z;
			movePlayerPacket.yaw = location.Yaw;
			movePlayerPacket.pitch = location.Pitch;
			movePlayerPacket.headYaw = location.HeadYaw;
			movePlayerPacket.mode = 1;
			movePlayerPacket.onGround = onGround;

			SendPacket(movePlayerPacket);
		}

		public void InitiateEncryption(byte[] serverKey, byte[] randomKeyToken)
		{

			try
			{
				ECPublicKeyParameters remotePublicKey = (ECPublicKeyParameters) PublicKeyFactory.CreateKey(serverKey);

				var handler = MessageHandler;

				ECDHBasicAgreement agreement = new ECDHBasicAgreement();
				agreement.Init(handler.CryptoContext.ClientKey.Private);
				byte[] secret;

				using (var sha = SHA256.Create())
				{
					secret = sha.ComputeHash(
						randomKeyToken.Concat(agreement.CalculateAgreement(remotePublicKey).ToByteArrayUnsigned())
						   .ToArray());
				}

				// Create a decrytor to perform the stream transform.

				IBufferedCipher decryptor = CipherUtilities.GetCipher("AES/CFB8/NoPadding");

				decryptor.Init(false, new ParametersWithIV(new KeyParameter(secret), secret.Take(16).ToArray()));

				IBufferedCipher encryptor = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
				encryptor.Init(true, new ParametersWithIV(new KeyParameter(secret), secret.Take(16).ToArray()));

				//Thread.Sleep(1250);
				
				handler.CryptoContext = new MiNET.Utils.CryptoContext
				{
					Decryptor = decryptor,
					Encryptor = encryptor,
					UseEncryption = true,
					Key = secret,
					ClientKey = handler.CryptoContext.ClientKey
				};
				//Thread.Sleep(1250);

				McpeClientToServerHandshake magic = McpeClientToServerHandshake.CreateObject();
				SendPacket(magic);

				//Session.FirstEncryptedMessage = Connection.Session.ReliableMessageNumber;
				Log.Info($"Encryption initiated!");
			}
			catch (Exception e)
			{
				Log.Error(e, $"Initiate encryption: {e.ToString()}");
			}
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

        private byte[] EncodeJwt(CertificateData certificateData, string b64Key, ECDsa signKey, bool doChain)
        {
	        string val = JWT.Encode(certificateData, signKey, JwsAlgorithm.ES384, new Dictionary<string, object> { { "x5u", b64Key } }, new JwtSettings()
            {
                JsonMapper = new JWTMapper()
            });

           // Log.Warn(JWT.Payload(val));

          //  Log.Warn(string.Join(";", JWT.Headers(val)));
          if (doChain)
          {
	          val = $@"{{ ""chain"": [""{val}""] }}";
          }

          return Encoding.UTF8.GetBytes(val);
        }

        private BoneName ParseBoneName(string name)
        {
	        if (Enum.TryParse(name, true, out BoneName boneName))
	        {
		        return boneName;
	        }

	        return BoneName.Unknown;
        }

        private byte[] EncodeSkinJwt(AsymmetricCipherKeyPair newKey, ECDsa signKey, string username, string x5u)
        {
	        #region Skin Json

	        var skinText =
		        "{\"Slim\":false,\"IsPersonaSkin\":false,\"IsPremiumSkin\":false,\"Cape\":{\"Id\":\"\",\"ImageHeight\":0,\"ImageWidth\":0,\"Data\":\"\",\"OnClassicSkin\":false},\"SkinId\":\"Standard_Custom\",\"ResourcePatch\":\"{\\\"geometry\\\" : {\\\"default\\\" : \\\"geometry.humanoid.custom.1587857794.1523\\\"}}\",\"SkinResourcePatch\":{\"Geometry\":{\"Default\":\"geometry.humanoid.custom.1587857794.1523\",\"animated_face\":null},\"persona_reset_resource_definitions\":false},\"Height\":128,\"Width\":128,\"Data\":\"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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACHgnX/m5iL/4eCdf+HgnX/h4J1/4eCdf+bmIv/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAG1pXf+HgnX/bWld/21pXf9taV3/bWld/4eCdf9taV3/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAG1pXf+bmIv/m5iL/5uYi/+bmIv/m5iL/4eCdf9taV3/bWld/21pXf9taV3/h4J1/4eCdf9taV3/bWld/21pXf9taV3/h4J1/5uYi/+bmIv/m5iL/5uYi/+bmIv/bWld/21pXf+bmIv/h4J1/4eCdf+HgnX/h4J1/5uYi/9taV3/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJuYi/+bmIv/m5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/5uYi/+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/5uYi/+bmIv/h4J1/7Wypv+HgnX/h4J1/4eCdf+HgnX/tbKm/4eCdf8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACbmIv/m5iL/7Wypv+1sqb/m5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAC1sqb/tbKm/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACbmIv/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/4eCdf+HgnX/h4J1/5uYi/+HgnX/tbKm/4eCdf+HgnX/h4J1/4eCdf+1sqb/h4J1/wAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/5uYi/+bmIv/tbKm/7Wypv/MyLv/zMi7/8zIu//MyLv/tbKm/7Wypv+bmIv/m5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALWypv+1sqb/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJuYi/+HgnX/bWld/4eCdf+HgnX/h4J1/4eCdf8eHiP/Hh4j/x4eI/8eHiP/Hh4j/x4eI/8eHiP/Hh4j/x4eI/8eHiP/h4J1/4eCdf+HgnX/h4J1/21pXf+HgnX/m5iL/4eCdf+bmIv/h4J1/4eCdf+HgnX/h4J1/5uYi/+HgnX/AAAAAAAAAAAAAAAAm5iL/7Wypv+bmIv/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/7Wypv+bmIv/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/4eCdf+HgnX/bWld/21pXf+HgnX/h4J1/x4eI/8eHiP/Hh4j/x4eI/8eHiP/Hh4j/x4eI/8eHiP/Hh4j/x4eI/+HgnX/h4J1/21pXf9taV3/h4J1/4eCdf+bmIv/h4J1/5uYi/9taV3/h4J1/4eCdf9taV3/m5iL/4eCdf8AAAAAAAAAAAAAAAAAAAAAAAAAAJuYi/+bmIv/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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAsPXL/LD1y/yw9cv8sPXL/LD1y/yUyVv+egkf/AAAAAAAAAAAAAAAAAAAAAAAAAACegkf/RFuZ/yUyVv8sPXL/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAbWld/4eCdf+HgnX/bWld/21pXf+HgnX/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACHgnX/bWld/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACw9cv8lMlb/JTJW/yUyVv8sPXL/JTJW/7WWWv8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACegkf/JTJW/yUyVv8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACHgnX/m5iL/5uYi/+HgnX/h4J1/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJuYi/+HgnX/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAnoJH/7WWWv+1llr/noJH/56CR/+egkf/tZZa/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAJ6CR/+egkf/noJH/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIeCdf+1sqb/tbKm/4eCdf+HgnX/tbKm/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAtbKm/4eCdf8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAh4J1/7Wypv+1sqb/h4J1/4eCdf+HgnX/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACHgnX/h4J1/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAm5iL/5uYi/8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABtaV3/h4J1/4eCdf9taV3/bWld/21pXf8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAG1pXf9taV3/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACbmIv/tbKm/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABROin/Ykkw/21pXf+HgnX/h4J1/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFE6Kf9ROin/UTop/2JJMP9iSTD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACw9cv+egkf/AAAAAAAAAACegkf/LD1y/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACw9cv84TIf/OEyH/zhMh/84TIf/LD1y/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//E8Ev//awD//2sA//8AAP//AAD//wAA/wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A/////3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/0OQ7/9DkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/8jwR//9rAP//awD//wAA//8AAP//AAD/AP///wD///8A////AP///wD///8A////AP///wD///8A////AP///wD/////cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//Q5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/xPBP//2sA//9rAP//AAD//wAA//8AAP8A////AP///wD///8A////AP///wD///8A////AP///wD///8A////AP////9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAPU5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO/+4/F///awD//2sA//8AAP//AAD//wAA/wD///8A////AP///wD///8A////AP///wD///8A////AP///wD///8A/////3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9DgN//Q5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/6UUg//8AAP//AAD//wAA//8AAP//AAD/AP///wD///8A////AP///wD///8A////AP///wD3//8A5///ANj//wDP////cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc//9x3P//cdz//3Hc/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD2OQ7/9jkO//Y5Dv/2OQ7/9jkO//Y5Dv/2OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/1OQ7/9TkO//U5Dv/wPRP/8D0V//U5Dv/1OA3/9DgN//E3Df/vNw3/8DcN//M4Df/oRyL//wAA//8AAP//AAD//wAA//8AAP8A6v//AOD//wDU//8Ayf//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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA8noU//N/Fv/zfhb/83sV//N8Fv/zfRX/830V//N+Ff/zfxb/838W//N/Fv/zfxb/838W//N/Fv/zfxb/8n8W/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADzeBP/834W//N6FP/zehT/838W//N8FP/zexT/830W//N/Fv/zfhb/83wW//N+Fv/zfhb/838W//N9Fv/yfxb/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAPN9Ff/zfxb/834W//N/Fv/zfhb/830W//N/Fv/zfhb/830W//N8Fv/zfRb/838W//N/Fv/zfxb/83wW//J+Fv8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA83wV//N9Fv/zfhb/834W//N9Fv/zfxb/838W//N+Fv/zfhb/838W//N/Fv/zfhb/83wW//N+Fv/zfhb/83ET/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP8AAAD/AAAA/wAAAP82NjYOAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA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AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==\",\"GeometryName\":null,\"GeometryData\":\"{\\\"format_version\\\":\\\"1.8.0\\\",\\\"geometry.humanoid\\\":{\\\"texturewidth\\\":64,\\\"textureheight\\\":64,\\\"bones\\\":[{\\\"name\\\":\\\"root\\\",\\\"pivot\\\":[0,0,0]},{\\\"name\\\":\\\"body\\\",\\\"parent\\\":\\\"root\\\",\\\"pivot\\\":[0,24,0],\\\"cubes\\\":[{\\\"origin\\\":[-4,12,-2],\\\"size\\\":[8,12,4],\\\"uv\\\":[16,16]}]},{\\\"name\\\":\\\"waist\\\",\\\"parent\\\":\\\"body\\\",\\\"neverRender\\\":true,\\\"pivot\\\":[0,12,0]},{\\\"name\\\":\\\"head\\\",\\\"parent\\\":\\\"body\\\",\\\"pivot\\\":[0,24,0],\\\"cubes\\\":[{\\\"origin\\\":[-4,24,-4],\\\"size\\\":[8,8,8],\\\"uv\\\":[0,0]}]},{\\\"name\\\":\\\"hat\\\",\\\"parent\\\":\\\"head\\\",\\\"pivot\\\":[0,24,0],\\\"cubes\\\":[{\\\"origin\\\":[-4,24,-4],\\\"size\\\":[8,8,8],\\\"uv\\\":[32,0],\\\"inflate\\\":0.5}],\\\"neverRender\\\":true},{\\\"name\\\":\\\"rightArm\\\",\\\"parent\\\":\\\"body\\\",\\\"pivot\\\":[-5,22,0],\\\"cubes\\\":[{\\\"origin\\\":[-8,12,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[40,16]}]},{\\\"name\\\":\\\"leftArm\\\",\\\"parent\\\":\\\"body\\\",\\\"pivot\\\":[5,22,0],\\\"cubes\\\":[{\\\"origin\\\":[4,12,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[40,16]}],\\\"mirror\\\":true},{\\\"name\\\":\\\"rightLeg\\\",\\\"parent\\\":\\\"root\\\",\\\"pivot\\\":[-1.9,12,0],\\\"cubes\\\":[{\\\"origin\\\":[-3.9,0,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[0,16]}]},{\\\"name\\\":\\\"leftLeg\\\",\\\"parent\\\":\\\"root\\\",\\\"pivot\\\":[1.9,12,0],\\\"cubes\\\":[{\\\"origin\\\":[-0.1,0,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[0,16]}],\\\"mirror\\\":true}]},\\\"geometry.humanoid.custom.1587857794.1523:geometry.humanoid\\\":{\\\"texturewidth\\\":128,\\\"textureheight\\\":128,\\\"bones\\\":[{\\\"name\\\":\\\"hat\\\",\\\"parent\\\":\\\"head\\\",\\\"neverRender\\\":false,\\\"pivot\\\":[0,24,0]},{\\\"name\\\":\\\"leftArm\\\",\\\"parent\\\":\\\"body\\\",\\\"reset\\\":true,\\\"mirror\\\":false,\\\"pivot\\\":[5,22,0],\\\"cubes\\\":[{\\\"origin\\\":[4,12,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[32,48]}]},{\\\"name\\\":\\\"rightArm\\\",\\\"parent\\\":\\\"body\\\",\\\"reset\\\":true,\\\"pivot\\\":[-5,22,0],\\\"cubes\\\":[{\\\"origin\\\":[-8,12,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[40,16]}]},{\\\"name\\\":\\\"rightItem\\\",\\\"pivot\\\":[-6,15,1],\\\"neverRender\\\":true,\\\"parent\\\":\\\"rightArm\\\"},{\\\"name\\\":\\\"leftSleeve\\\",\\\"parent\\\":\\\"leftArm\\\",\\\"pivot\\\":[5,22,0],\\\"cubes\\\":[{\\\"origin\\\":[4,12,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[48,48],\\\"inflate\\\":0.25}]},{\\\"name\\\":\\\"rightSleeve\\\",\\\"parent\\\":\\\"rightArm\\\",\\\"pivot\\\":[-5,22,0],\\\"cubes\\\":[{\\\"origin\\\":[-8,12,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[40,32],\\\"inflate\\\":0.25}]},{\\\"name\\\":\\\"leftLeg\\\",\\\"parent\\\":\\\"root\\\",\\\"reset\\\":true,\\\"mirror\\\":false,\\\"pivot\\\":[1.9,12,0],\\\"cubes\\\":[{\\\"origin\\\":[-0.1,0,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[16,48]}]},{\\\"name\\\":\\\"leftPants\\\",\\\"parent\\\":\\\"leftLeg\\\",\\\"pivot\\\":[1.9,12,0],\\\"cubes\\\":[{\\\"origin\\\":[-0.1,0,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[0,48],\\\"inflate\\\":0.25}]},{\\\"name\\\":\\\"rightPants\\\",\\\"parent\\\":\\\"rightLeg\\\",\\\"pivot\\\":[-1.9,12,0],\\\"cubes\\\":[{\\\"origin\\\":[-3.9,0,-2],\\\"size\\\":[4,12,4],\\\"uv\\\":[0,32],\\\"inflate\\\":0.25}]},{\\\"name\\\":\\\"jacket\\\",\\\"parent\\\":\\\"body\\\",\\\"pivot\\\":[0,24,0],\\\"cubes\\\":[{\\\"origin\\\":[-4,12,-2],\\\"size\\\":[8,12,4],\\\"uv\\\":[16,32],\\\"inflate\\\":0.25}]},{\\\"name\\\":\\\"head\\\",\\\"parent\\\":\\\"body\\\",\\\"pivot\\\":[0,24,0],\\\"cubes\\\":[{\\\"origin\\\":[-4,24,-4],\\\"size\\\":[8,8,8],\\\"uv\\\":[0,0]},{\\\"origin\\\":[-6,32,-2],\\\"size\\\":[12,1,4],\\\"uv\\\":[0,67]},{\\\"origin\\\":[-7,33,-2],\\\"size\\\":[1,1,4],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,33,-2],\\\"size\\\":[1,3,4],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,36,-2],\\\"size\\\":[6,1,4],\\\"uv\\\":[1,68.5]},{\\\"origin\\\":[-2,37,-2],\\\"size\\\":[4,1,4],\\\"uv\\\":[1,68.5]},{\\\"origin\\\":[-1,37,-2],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-1,38,-2],\\\"size\\\":[2,1,4],\\\"uv\\\":[2,71]},{\\\"origin\\\":[0,37,-2],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[3,33,-2],\\\"size\\\":[1,3,4],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-1,37,-4],\\\"size\\\":[2,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,36,-4],\\\"size\\\":[4,1,2],\\\"uv\\\":[1,68.5]},{\\\"origin\\\":[-3,35,-4],\\\"size\\\":[6,1,2],\\\"uv\\\":[1,68.5]},{\\\"origin\\\":[3,32,-4],\\\"size\\\":[1,3,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,32,-4],\\\"size\\\":[1,3,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-6,31,-4],\\\"size\\\":[12,1,2],\\\"uv\\\":[0,72]},{\\\"origin\\\":[-7,32,-4],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-1,37,2],\\\"size\\\":[2,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,36,2],\\\"size\\\":[4,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,35,2],\\\"size\\\":[6,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,32,2],\\\"size\\\":[8,3,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-6,31,2],\\\"size\\\":[12,1,2],\\\"uv\\\":[1,74]},{\\\"origin\\\":[-7,32,2],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-6,32,-6],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-5,31,-6],\\\"size\\\":[10,1,2],\\\"uv\\\":[2,76]},{\\\"origin\\\":[-4,32,-5],\\\"size\\\":[8,2,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,34,-5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,35,-5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-1,36,-5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[0,36,-5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[1,35,-5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[2,34,-5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-1,36,4],\\\"size\\\":[2,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,35,4],\\\"size\\\":[4,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,34,4],\\\"size\\\":[6,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,32,4],\\\"size\\\":[8,2,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-6,31,4],\\\"size\\\":[12,1,1],\\\"uv\\\":[0,73]},{\\\"origin\\\":[-7,32,4],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,31,-6],\\\"size\\\":[8,2,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,31,-7],\\\"size\\\":[6,3,2],\\\"uv\\\":[4,74]},{\\\"origin\\\":[-2,31,-6],\\\"size\\\":[1,4,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[-1,32,-6],\\\"size\\\":[2,4,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[1,31,-6],\\\"size\\\":[1,4,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[-3,30,5],\\\"size\\\":[6,4,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,31,5],\\\"size\\\":[4,4,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-1,32,5],\\\"size\\\":[2,4,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,30,5],\\\"size\\\":[8,3,1],\\\"uv\\\":[9,73]},{\\\"origin\\\":[-1,31,6],\\\"size\\\":[2,4,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,30,6],\\\"size\\\":[4,4,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[6,33,-2],\\\"size\\\":[1,1,4],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[6,32,-4],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[5,32,-6],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[6,32,2],\\\"size\\\":[1,1,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[6,32,4],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[6,31,5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-6,30,5],\\\"size\\\":[12,1,1],\\\"uv\\\":[1,75]},{\\\"origin\\\":[-7,31,5],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[6,30,6],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[4,29,6],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[-6,29,6],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[-7,30,6],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[5,30,7],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-5,29,7],\\\"size\\\":[10,1,1],\\\"uv\\\":[2,73]},{\\\"origin\\\":[-6,30,7],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-5,29,8],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,28,8],\\\"size\\\":[8,1,1],\\\"uv\\\":[9,75]},{\\\"origin\\\":[4,29,8],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,29,9],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,28,9],\\\"size\\\":[6,1,1],\\\"uv\\\":[11,71]},{\\\"origin\\\":[3,29,9],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[2,29,10],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,29,10],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,28,10],\\\"size\\\":[4,1,1],\\\"uv\\\":[20,74]},{\\\"origin\\\":[-4,29,6],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[2,29,6],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[0,29,6],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[-2,29,6],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[-1,30,7],\\\"size\\\":[2,3,2],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,30,7],\\\"size\\\":[4,2,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,30,6],\\\"size\\\":[6,2,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,30,6],\\\"size\\\":[8,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-5,32,-7],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-4,31,-7],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[4,32,-7],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[2,31,-7],\\\"size\\\":[2,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[3,32,-8],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,31,-8],\\\"size\\\":[6,1,1],\\\"uv\\\":[14,74]},{\\\"origin\\\":[-4,32,-8],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-3,32,-9],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]},{\\\"origin\\\":[-2,31,-9],\\\"size\\\":[4,1,1],\\\"uv\\\":[23,75.5]},{\\\"origin\\\":[2,32,-9],\\\"size\\\":[1,1,1],\\\"uv\\\":[1,71.5]}]}]}}\",\"ArmSize\":\"\",\"SkinColor\":\"\",\"AnimationData\":\"\",\"Animations\":[],\"PersonaPieces\":[],\"SkinPieces\":[]}";
	        
	        #endregion
	        
	        Skin skin = null;

	        if (Alex.PlayerModel != null && Alex.PlayerTexture != null)
	        {
		        var model = Alex.PlayerModel;

		        byte[] skinData;
		        using (MemoryStream ms = new MemoryStream())
		        {
			        if (Alex.PlayerTexture.TryGetSinglePixelSpan(out var span))
			        {
				        foreach (var value in span)
				        {
					        ms.WriteByte(value.R);
					        ms.WriteByte(value.G);
					        ms.WriteByte(value.B);
					        ms.WriteByte(value.A);
				        }
			        }
			        skinData = ms.ToArray();
		        }
		        
		        GeometryModel mm       = new GeometryModel();
		        mm.Geometry.Add(model.Description.Identifier, model);

		        var modelIdentifier = model.Description.Identifier;
		        
		        skin = new Skin()
		        {
			        Cape = new Cape()
			        {
				       Data = new byte[0],
				       Id = "",
				       ImageHeight = 0,
				       ImageWidth = 0,
				       OnClassicSkin = false
			        },
			        SkinId = modelIdentifier,
			        ResourcePatch =
				        Convert.ToBase64String(
					        Encoding.UTF8.GetBytes(MCJsonConvert.SerializeObject(new SkinResourcePatch()
					        {
						        Geometry = 	new GeometryIdentifier()
						        {
							         Default = modelIdentifier
						        }
					        }))),
			        Width = (int) Alex.PlayerTexture.Width,
			        Height = (int) Alex.PlayerTexture.Height,
			        Data = skinData,
			        GeometryName = modelIdentifier,
			        GeometryData =
				        Convert.ToBase64String(
					        Encoding.UTF8.GetBytes(MCJsonConvert.SerializeObject(mm))),
			        AnimationData = "",
			        IsPremiumSkin = false,
			        IsPersonaSkin = false,
			        Slim = true
		        };
	        }
	        else
	       {
		       dynamic payload = JObject.Parse(skinText);
		       
		        skin = new Skin()
		        {
			        Cape = new Cape()
			        {
				        Data = Convert.FromBase64String((string) payload.Cape.Data ?? string.Empty),
				        Id = payload.Cape.Id,
				        ImageHeight = payload.Cape.ImageHeight,
				        ImageWidth = payload.Cape.ImageWidth,
				        OnClassicSkin = payload.Cape.OnClassicSkin,
			        },
			        SkinId = payload.SkinId,
			        ResourcePatch =
				        Convert.ToBase64String(
					        Encoding.UTF8.GetBytes((string) payload.ResourcePatch ?? string.Empty)),
			        Width = payload.Width,
			        Height = payload.Height,
			        Data = Convert.FromBase64String((string) payload.Data ?? string.Empty),
			        GeometryData =
				        Convert.ToBase64String(
					        Encoding.UTF8.GetBytes((string) payload.SkinGeometryData ?? string.Empty)),
			        AnimationData = payload.AnimationData,
			        IsPremiumSkin = payload.IsPremiumSkin,
			        IsPersonaSkin = payload.IsPersonaSkin,
		        };
	        }

	        var serialized = JsonConvert.SerializeObject(
		        new BedrockSkinData(skin)
		        {
			        SelfSignedID = Alex.Resources.DeviceID,
			        ClientRandomId = new Random().Next(),
			        LanguageCode = Alex.GuiRenderer.Language.Code.Replace("-", "_"),
			        // Alex.Services.GetService<IOptionsProvider>().AlexOptions.MiscelaneousOptions.Language.Value,
			        ServerAddress = $"{ServerEndpoint.Address.ToString()}:{ServerEndpoint.Port.ToString()}",
			        ThirdPartyName = username,
			        DeviceId = Alex.Resources.DeviceID,
			        GameVersion = McpeProtocolInfo.GameVersion,
			        
		        },
		        new JsonSerializerSettings()
		        {
			        ContractResolver =
				        new DefaultContractResolver {NamingStrategy = new DefaultNamingStrategy() { }},
			        DefaultValueHandling = DefaultValueHandling.Include
		        });
	        
	   //     Log.Info(serialized);

	        string val = JWT.Encode(serialized, signKey, JwsAlgorithm.ES384, new Dictionary<string, object> { { "x5u", x5u } }, new JwtSettings()
            {
                JsonMapper = new JWTMapper()
            });

            return Encoding.UTF8.GetBytes(val);
        }

		public World World { get; set; } 
		public System.Numerics.Vector3 SpawnPoint { get; set; } = System.Numerics.Vector3.Zero;
		public LevelInfo LevelInfo { get; } = new LevelInfo();

		public override void EntityAction(int entityId, EntityAction action)
		{
			BlockCoordinates? coordinates = null;
			if (entityId == World.Player.EntityId)
			{
				coordinates = World.Player.KnownPosition.GetCoordinates3D();
			}

			PlayerAction translated;
			switch (action)
			{
				case API.Utils.EntityAction.StartSneaking:
					translated = PlayerAction.StartSneak;
					break;
				case API.Utils.EntityAction.StopSneaking:
					translated = PlayerAction.StopSneak;
					break;

				case API.Utils.EntityAction.StartSprinting:
					translated = PlayerAction.StartSprint;
					break;
				case API.Utils.EntityAction.StopSprinting:
					translated = PlayerAction.StopSprint;
					break;
				
				case API.Utils.EntityAction.Jump:
					translated = PlayerAction.Jump;
					break;

				default:
					return;
			}
			
			SendPlayerAction(translated, coordinates, null);
		}

		/// <inheritdoc />
		public override void PlayerAnimate(PlayerAnimations animation)
		{
			McpeAnimate animate = McpeAnimate.CreateObject();
			animate.runtimeEntityId = EntityId;
			
			switch (animation)
			{
				case PlayerAnimations.SwingRightArm:
				case PlayerAnimations.SwingLeftArm:
					animate.actionId = 1;
					break;

				default:
					return;
			}
			
			SendPacket(animate);
		}

		private void SendChat(string message)
		{
			McpeText text = McpeText.CreateObject();
			text.message = message;
			text.type = (byte) MessageType.Chat;
			
			SendPacket(text);
		}

		public void SendPlayerAction(PlayerAction action, BlockCoordinates? coordinates, int? blockFace )
		{
			McpePlayerAction packet = McpePlayerAction.CreateObject();
			packet.actionId = (int) action;
			
			if (coordinates.HasValue)
				packet.coordinates = new MiNET.Utils.BlockCoordinates(coordinates.Value.X, 
					coordinates.Value.Y, coordinates.Value.Z);

			if (blockFace.HasValue)
				packet.face = blockFace.Value;
			
			SendPacket(packet);
		}
		
	    public override void PlayerDigging(DiggingStatus status, BlockCoordinates position, API.Blocks.BlockFace face, Vector3 cursorPosition)
	    {
            if (World?.Player is Entities.Player player)
            {
                var item = player.Inventory[player.Inventory.SelectedSlot];
                if (status == DiggingStatus.Started)
                {
                    SendPlayerAction(PlayerAction.StartBreak, position, (int) face);
                }
                else if (status == DiggingStatus.Finished)
                {
                    SendPlayerAction(PlayerAction.StopBreak, position, (int) face);
                    var packet = McpeInventoryTransaction.CreateObject();
                   /* packet.transaction = new Transaction()*/
                   packet.transaction = new ItemUseTransaction()
                    {
                        ActionType = McpeInventoryTransaction.ItemUseAction.Destroy,
                        ClickPosition =
                            new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
                       // TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
                       // EntityId = NetworkEntityId,
                        Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
                        Face = (int) ConvertBlockFace(face),
                        Slot = player.Inventory.SelectedSlot,
                        //Item = item.
                        Item = MiNET.Items.ItemFactory.GetItem(item.Id, item.Meta, item.Count)
                        
                    };

                   SendPacket(packet);
                }
                else if (status == DiggingStatus.Cancelled)
                {
                    SendPlayerAction(PlayerAction.AbortBreak, position, (int) face);
                }
                else if (status == DiggingStatus.DropItem)
                {
	                SendPlayerAction(PlayerAction.DropItem, position, (int) face);
                }
            }
        }

	    public static MiNET.Items.Item GetMiNETItem(Item item)
	    {
		    var minetItem = MiNET.Items.ItemFactory.GetItem(item.Id, item.Meta, item.Count);

		    if (minetItem.Id == 0)
		    {
			    if (MiNET.Worlds.AnvilWorldProvider.Convert.TryGetValue(item.Id, out var val))
			    {
				    var id   = val.Item1;
				    var meta = val.Item2(id, (byte) item.Meta);

				    minetItem = MiNET.Items.ItemFactory.GetItem((short) id, meta, item.Count);
			    }
		    }

		    minetItem.ExtraData = item.Nbt;
		    minetItem.UniqueId = item.StackID;
		    
		    return minetItem;
	    }

	    private MiNET.BlockFace ConvertBlockFace(API.Blocks.BlockFace face)
	    {
		    MiNET.BlockFace updatedFace =MiNET.BlockFace.None;

		    switch (face)
		    {
			    case BlockFace.Down:
				    updatedFace = MiNET.BlockFace.Down;
				    break;

			    case BlockFace.Up:
				    updatedFace = MiNET.BlockFace.Up;
				    break;

			    case BlockFace.East:
				    updatedFace = MiNET.BlockFace.East;
				    break;

			    case BlockFace.West:
				    updatedFace = MiNET.BlockFace.West;
				    break;

			    case BlockFace.North:
				    updatedFace = MiNET.BlockFace.North;
				    break;

			    case BlockFace.South:
				    updatedFace = MiNET.BlockFace.Down;
				    break;

			    case BlockFace.None:
				    updatedFace = MiNET.BlockFace.None;
				    break;
		    }

		    return updatedFace;
	    }
	    
	    public override void BlockPlaced(BlockCoordinates position, API.Blocks.BlockFace face, int hand, int slot, Vector3 cursorPosition, Entity entity)
	    {
		    if (entity is Player p)
		    {
			    var itemInHand = p.Inventory[slot];
			    var minetItem = GetMiNETItem(itemInHand);
			    
			    Log.Info($"Placing block, slot={slot} InHand={itemInHand.ToString()} face={face} pos={position}");

			    var packet = McpeInventoryTransaction.CreateObject();
			    packet.transaction = new ItemUseTransaction()
			    {
				    ActionType = (int) McpeInventoryTransaction.ItemUseAction.Place,
				    ClickPosition =
					    new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
				    //TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
				    // = NetworkEntityId,
				    Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
				    Face = (int) ConvertBlockFace(face),
				    TransactionRecords = new List<TransactionRecord>()
				    {
						new WorldInteractionTransactionRecord()
						{
							//StackNetworkId = 
						}
				    },
				    Item = minetItem,
				    FromPosition = new System.Numerics.Vector3(p.KnownPosition.X, p.KnownPosition.Y, p.KnownPosition.Z),
				    Slot = slot,
				    BlockRuntimeId = ChunkProcessor.BlockStateMap.FirstOrDefault(x => x.Value.Id == itemInHand.Id && x.Value.Data == itemInHand.Meta).Key
				    //BlockRuntimeId = 
			    };

			    SendPacket(packet);
		    }
	    }

		public override void EntityInteraction(Entity player, Entity target,
		    ItemUseOnEntityAction action, int hand, int slot)
	    {
		    if (player is Player p)
		    {
			    //p.Inventory[p.Inventory.SelectedSlot]
			    //var itemInHand = p.Inventory.MainHand;

			    // WorldProvider?.GetChatReceiver?.Receive(new ChatObject($"(CLIENT) Hit entity: {target.EntityId} | Action: {action.ToString()} | Item: {itemInHand.Id}:{itemInHand.Meta} ({itemInHand.Name})"));

			  //  var item = GetMiNETItem(itemInHand); //MiNET.Items.ItemFactory.GetItem(itemInHand.Id, itemInHand.Meta, itemInHand.Count);
			  //  item.Metadata = itemInHand.Meta;
			  //  item.ExtraData = itemInHand.Nbt;
			   // item.Count = (byte) itemInHand.Count;

			   McpeInventoryTransaction.ItemUseOnEntityAction realAction;

			   switch (action)
			   {
				   case ItemUseOnEntityAction.Interact:
					   realAction = McpeInventoryTransaction.ItemUseOnEntityAction.Interact;
					   break;

				   case ItemUseOnEntityAction.Attack:
					   realAction = McpeInventoryTransaction.ItemUseOnEntityAction.Attack;
					   break;

				   default:
					   realAction = McpeInventoryTransaction.ItemUseOnEntityAction.ItemInteract;
					   break;
			   }
			   
			   if (p.Inventory is ItemStackInventory isi)
			   {
				   var packet = McpeInventoryTransaction.CreateObject();

				   packet.transaction = new ItemUseOnEntityTransaction()
				   {
					   ActionType = realAction,
					//   Item = isi.GetOriginal(slot),
					Item = GetMiNETItem(p.Inventory[slot]),
					   EntityId = target.EntityId,
					   Slot = slot,
					   FromPosition = new System.Numerics.Vector3(p.KnownPosition.X, p.KnownPosition.Y, p.KnownPosition.Z)
				   };

				   SendPacket(packet);
			   }
			   else
			   {
				   var packet = McpeInventoryTransaction.CreateObject();

				   packet.transaction = new ItemUseOnEntityTransaction()
				   {
					   ActionType = realAction,
					   Item = GetMiNETItem(p.Inventory[slot]),
					   EntityId = target.EntityId,
					   Slot = slot,
					   FromPosition = new System.Numerics.Vector3(p.KnownPosition.X, p.KnownPosition.Y, p.KnownPosition.Z)
				   };

				   SendPacket(packet);
			   }
		    }
	    }

	    public override void WorldInteraction(Entity entity, BlockCoordinates position, API.Blocks.BlockFace face, int hand, int slot, Vector3 cursorPosition)
	    {
		    MiNET.Items.Item minetItem;// = GetMiNETItem(item);
		     var orig = entity.Inventory[slot];
            			    minetItem = GetMiNETItem(orig);
		   /* if (entity.Inventory is ItemStackInventory isi)
		    {
			    minetItem = isi.GetOriginal(slot);
		    }
		    else
		    {
			    var orig = entity.Inventory[slot];
			    minetItem = GetMiNETItem(orig);
		    }*/
		    
		    var packet = McpeInventoryTransaction.CreateObject();
		    packet.transaction = new ItemUseTransaction()
		    {
			    ActionType = McpeInventoryTransaction.ItemUseAction.Clickblock,
			    ClickPosition =
				    new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
			  //  TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
			   // EntityId = NetworkEntityId,
			    Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
			    Face = (int) ConvertBlockFace(face),
			    Item = minetItem,
			    Slot = slot,
			    FromPosition = new System.Numerics.Vector3(entity.KnownPosition.X, entity.KnownPosition.Y, entity.KnownPosition.Z),
			    HasNetworkIds = true,
			    TransactionRecords = new List<TransactionRecord>()
			    {
				    
			    }
			  //  BlockRuntimeId = 
			  //  BlockRuntimeId = 
		    };

		  SendPacket(packet);
	    }

	    public override void UseItem(Item item, int hand, ItemUseAction action, BlockCoordinates position, API.Blocks.BlockFace face, Vector3 cursorPosition)
	    {
		    MiNET.Items.Item minetItem;// = GetMiNETItem(item);
		    minetItem = GetMiNETItem(item);
		   /* if (World.Player.Inventory is ItemStackInventory isi)
		    {
			    minetItem = isi.GetOriginal(isi.SelectedSlot);
		    }
		    else
		    {
			    minetItem = GetMiNETItem(item);
		    }*/

		   // Log.Info($"Use Item: {item} ({minetItem}) hand: {hand} Action: {action}");
		    
		    McpeInventoryTransaction.ItemUseAction useAction = McpeInventoryTransaction.ItemUseAction.Use;
		    switch (action)
		    {
			    case ItemUseAction.Use:
				    break;
			    
			    case ItemUseAction.ClickBlock:
			    case ItemUseAction.RightClickBlock:
				    useAction = McpeInventoryTransaction.ItemUseAction.Clickblock;
				    break;
			    
			    case ItemUseAction.ClickAir:
			    case ItemUseAction.RightClickAir:
				    useAction = McpeInventoryTransaction.ItemUseAction.Clickair;
				    break;
		    }
		    
		    var packet = McpeInventoryTransaction.CreateObject();
		   // packet.
		    packet.transaction = new ItemUseTransaction()
		    {
			    ActionType = useAction,
			    ClickPosition =
				    new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
			    //  TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
			    // EntityId = NetworkEntityId,
			    Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
			    Face = (int) ConvertBlockFace(face),
			    Item = minetItem,
			    Slot = World.Player.Inventory.SelectedSlot,
			    FromPosition = new System.Numerics.Vector3(World.Player.KnownPosition.X, World.Player.KnownPosition.Y, World.Player.KnownPosition.Z),
			   // Item = minetItem,
			   // Slot = World.Player.Inventory.SelectedSlot
		    };

		    SendPacket(packet);
		    
			//Log.Warn("TODO: Implement UseItem");
		}

		public override void HeldItemChanged(Item item, short slot)
		{
		//	var minetItem = GetMiNETItem(item);
			
			McpeMobEquipment packet = new McpeMobEquipment();
			packet.selectedSlot = (byte) slot;
			packet.slot = (byte) slot;

			/*if (World.Player.Inventory is ItemStackInventory isi)
			{
				packet.item = isi.GetOriginal(slot);
			}
			else
			{
				packet.item = GetMiNETItem(item);
			}*/
			packet.item = GetMiNETItem(item);

			SendPacket(packet);
			
			//Log.Warn($"Held item slot changed: {slot} | Inventor: ");
		}

		public override void Close()
		{
			ResourcePackManager?.Dispose();
			ResourcePackManager = null;
			
			if (PacketHandler != null)
				PacketHandler.ReportPackets();
			
			CancellationTokenSource?.Cancel();
			SendDisconnectionNotification();

			Task.Delay(500).ContinueWith(task =>
			{

				try
				{
					Connection.Stop();
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Exception while closing connection.");
				}
				finally
				{
					ThroughPut?.Change(Timeout.Infinite, Timeout.Infinite);
					//Connection.ConnectionInfo.ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);
				}
			});

			//Task.Delay(500).ContinueWith(task => { base.StopClient(); });
		}

		/// <inheritdoc />
		public override void SendChatMessage(ChatObject msg)
		{
			var message = msg.RawMessage; 
			if (message[0] == '/')
			{
				McpeCommandRequest commandRequest = McpeCommandRequest.CreateObject();
				commandRequest.command = message;
				commandRequest.unknownUuid = new MiNET.Utils.UUID(Guid.NewGuid().ToString());
				SendPacket(commandRequest);
			}
			else
			{
				SendChat(message);
			}
		}

		public void RequestChunkRadius(int radius)
		{
			var packet = McpeRequestChunkRadius.CreateObject();
			packet.chunkRadius = radius;

			SendPacket(packet);
			
			//if (PlayerStatus != 3 && GameStarted)
				//MarkAsInitialized();
		}

		public void SendDisconnectionNotification()
		{
			SendPacket(new DisconnectionNotification());
		}

		public void Dispose()
		{
			foreach (var disposable in _disposables.ToArray())
			{
				disposable.Dispose();
			}
			
			_disposables.Clear();
			
			//ChunkProcessor.Instance = null;
			Close();
			//WorkerThreadPool.Dispose();
			//_threadPool.WaitForThreadsExit();
		}
	}

	public enum DisconnectReason
	{
		Network,
		Kicked,
		Unknown
	}
}
