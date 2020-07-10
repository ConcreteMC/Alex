using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API;
using Alex.API.Data.Options;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Net;
using Alex.Net.Bedrock;
using Alex.Net.Bedrock.Raknet;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
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
using SixLabors.ImageSharp.Formats.Png;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ConnectionInfo = Alex.API.Network.ConnectionInfo;
using DedicatedThreadPool = MiNET.Utils.DedicatedThreadPool;
using Item = Alex.Items.Item;
using LevelInfo = MiNET.Worlds.LevelInfo;
using Player = Alex.Entities.Player;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockMotd
	{
		public string Edition;
		public string MOTD;
		public int MaxPlayers;
		public int Players;
		public int ProtocolVersion;
		public string ClientVersion;
		public IPEndPoint ServerEndpoint;
		public long Latency;

		public BedrockMotd(string raw)
		{
			if (string.IsNullOrWhiteSpace(raw)) return;

			var split = raw.Split(';');
			int i = 0;
			Edition = split[i++];
			MOTD = split[i++];

			if (int.TryParse(split[i++], out int protocolVersion))
			{
				ProtocolVersion = protocolVersion;
			}
			
			ClientVersion = split[i++];

			if (int.TryParse(split[i++], out int players))
			{
				Players = players;
			}

			if (int.TryParse(split[i++], out int maxplayers))
			{
				MaxPlayers = maxplayers;
			}
		}
	}
	public class BedrockClient : NetworkProvider, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClient));
		
		private ManualResetEventSlim ConnectionAcceptedWaitHandle { get; set; }
		public BedrockWorldProvider WorldProvider { get; }
		public EventHandler<BedrockMotd> OnMotdReceivedHandler;
		//public BedrockMotd KnownMotd = new BedrockMotd(string.Empty);

        private Alex Alex { get; }
        private IOptionsProvider OptionsProvider { get; }
        private XboxAuthService XboxAuthService { get; }
        private AlexOptions Options => OptionsProvider.AlexOptions;
        private DedicatedThreadPool _threadPool;
        
        public PlayerProfile PlayerProfile { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        
        public McpeNetworkChunkPublisherUpdate LastChunkPublish { get; set; }
        public bool HasSpawned { get; set; }
        public AutoResetEvent PlayerStatusChanged { get; set; } = new AutoResetEvent(false);
        public AutoResetEvent ChangeDimensionResetEvent = new AutoResetEvent(false);
        private IEventDispatcher EventDispatcher { get; }
        public RakConnection Connection { get; }
        private MessageHandler MessageHandler { get; set; }
        private RakSession Session { get; set; }//=> Connection.ConnectionInfo.RakSessions.Values.FirstOrDefault();
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

        public bool GameStarted { get; set; } = false;
        private ChunkProcessor ChunkProcessor { get; }
		public BedrockClient(Alex alex, IEventDispatcher eventDispatcher, IPEndPoint endpoint, PlayerProfile playerProfile, DedicatedThreadPool threadPool, BedrockWorldProvider wp)
		{
			PacketFactory.CustomPacketFactory = new AlexPacketFactory();
			
			PlayerProfile = playerProfile;
			CancellationTokenSource = new CancellationTokenSource();
			
            Alex = alex;
			WorldProvider = wp;
			OptionsProvider = alex.Services.GetRequiredService<IOptionsProvider>();
			XboxAuthService = alex.Services.GetRequiredService<XboxAuthService>();
			
			ChunkRadius = Options.VideoOptions.RenderDistance;
			
			Options.VideoOptions.RenderDistance.Bind(RenderDistanceChanged);
			Options.VideoOptions.ClientSideLighting.Bind(ClientSideLightingChanged);
			_threadPool = threadPool;

			ChunkProcessor = new ChunkProcessor(this, alex.ThreadPool,
				alex.Services.GetRequiredService<IOptionsProvider>().AlexOptions.MiscelaneousOptions.ServerSideLighting,
				CancellationTokenSource.Token);

			ChunkProcessor.ClientSideLighting = Options.VideoOptions.ClientSideLighting;
			
			Connection = new RakConnection(new IPEndPoint(IPAddress.Any, 0), new GreyListManager(), new MotdProvider(), threadPool);
			ServerEndpoint = endpoint;

			Connection.CustomMessageHandlerFactory = session =>
			{
				Log.Info($"Requesting sessions...  {Connection.ConnectionInfo.RakSessions.Count}");

				bool hasSession = Session != null;
				
				if (!hasSession)
				{
					Session = session;
				}

				session.Username = playerProfile.Username;
				session.ConnectionInfo.RakSessions.Clear();

				var handler = new MessageHandler(session, new BedrockClientPacketHandler(this, eventDispatcher, wp, playerProfile, alex, CancellationTokenSource.Token, ChunkProcessor));
				if (!hasSession)
				{
					handler.ConnectionAction = () =>
					{
						ConnectionAcceptedWaitHandle?.Set();
						SendAlexLogin(playerProfile.Username);
					};

					handler.DisconnectedAction = (reason, sendDisconnect) =>
					{
						Log.Warn($"Got disconnected from server: {reason}");
						ShowDisconnect(reason);
					};
					
					MessageHandler = handler;
				}

				return handler;
			};

			EventDispatcher = eventDispatcher;
		}

		private void ClientSideLightingChanged(bool oldvalue, bool newvalue)
		{
			ChunkProcessor.ClientSideLighting = newvalue;
		}

		private bool Starting { get; set; } = false;
		private DateTime StartTime { get; set; }
		public void Start(ManualResetEventSlim resetEvent)
		{
			if (Starting)
				return;

			Starting = true;

			StartTime = DateTime.UtcNow;
		//	var player = WorldReceiver.Player;
			
			//player.Inventory.CursorChanged += InventoryOnCursorChanged;
		//	player.Inventory.SlotChanged += InventoryOnSlotChanged;
			
			
			EventDispatcher?.RegisterEvents(this);
			
			ConnectionAcceptedWaitHandle = resetEvent;
			
			ThreadPool.QueueUserWorkItem(o =>
			{
				if (!Connection.AutoConnect)
				{
					Connection.ConnectionInfo.ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);

					if (TryLocate(ServerEndpoint, out var serverInfo))
					{
						OnMotdReceivedHandler?.Invoke(this, new BedrockMotd(serverInfo.serverName)
						{
							ServerEndpoint = serverInfo.serverEndPoint,
							Latency = serverInfo.ping
						});
						resetEvent.Set();
					}
				}
				else
				{
					Connection.ConnectionInfo.ThroughPut = new Timer(
						state =>
						{
							Connection.ConnectionInfo.NumberOfPlayers = Connection.ConnectionInfo.RakSessions.Count;
							Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfDeniedConnectionRequestsPerSecond, 0);
							long   packetSizeOut = Interlocked.Exchange(ref Connection.ConnectionInfo.TotalPacketSizeOutPerSecond, 0L);
							long   packetSizeIn = Interlocked.Exchange(ref Connection.ConnectionInfo.TotalPacketSizeInPerSecond, 0L);
							double throughtPutOut = (double) (packetSizeOut * 8L) / 1000000.0;
							double throughPutIn = (double) (packetSizeIn * 8L) / 1000000.0;
							long   packetCountOut = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfPacketsOutPerSecond, 0L);
							long   packetCountIn = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfPacketsInPerSecond, 0L);
							
							long ackReceived  = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfAckReceive, 0L);
							long ackSent  = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfAckSent, 0L);
							long nakReceive  = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfNakReceive, 0L);
							long resends = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfResends, 0L);
							long fails = Interlocked.Exchange(ref Connection.ConnectionInfo.NumberOfFails, 0L);

							string str = string.Format("Pkt in/out(#/s) {0}/{1}, ", (object) packetCountIn, (object) packetCountOut)
							             + string.Format(
								             "ACK(in-out)/NAK/RSND/FTO(#/s) ({0}-{1})/{2}/{3}/{4}, ", (object) ackReceived,
								             (object) ackSent, (object) nakReceive, (object) resends, (object) fails)
							             + string.Format("THR in/out(Mbps) {0:F}/{1:F}, ", (object) throughPutIn, (object) throughtPutOut)
							             + string.Format(
								             "PktSz Total in/out(B/s){0}/{1}, ", (object) packetSizeIn, (object) packetSizeOut);

							if (Config.GetProperty("ServerInfoInTitle", false))
								Console.Title = str;
							else
								Log.Info(str);
								
							_connectionInfo = new ConnectionInfo(StartTime, CustomConnectedPong.Latency, nakReceive, ackReceived, ackSent, fails, resends, packetSizeIn, packetSizeOut);
						}, null, 1000, 1000);
					
					if (Connection.TryConnect(ServerEndpoint, 1))
					{
						
						//resetEvent.Set();
					}
				}
			});

		}
		
		public bool TryLocate(
			IPEndPoint targetEndPoint,
			out (IPEndPoint serverEndPoint, string serverName, long ping) serverInfo,
			int numberOfAttempts = 2147483647)
		{
			Stopwatch sw = new Stopwatch();
			this.Connection.Start();
			bool autoConnect = this.Connection.AutoConnect;
			this.Connection.AutoConnect = false;
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
			this.Connection.AutoConnect = autoConnect;
			return this.Connection.FoundServer;
		}
		
		private void SendUnconnectedPingInternal(IPEndPoint targetEndPoint)
		{
			byte[] data = new UnconnectedPing()
			{
				pingId = Stopwatch.GetTimestamp(),
				guid = this.Connection._rakOfflineHandler.ClientGuid
			}.Encode();
			
			if (targetEndPoint != null)
				this.Connection.SendData(data, targetEndPoint);
			else
				this.Connection.SendData(data, new IPEndPoint(IPAddress.Broadcast, 19132));
		}

		private ConnectionInfo _connectionInfo = new ConnectionInfo(DateTime.UtcNow, 0, 0,0,0,0,0,0,0);
		public override ConnectionInfo GetConnectionInfo()
		{
			return _connectionInfo;
			var conn = Session?.ConnectionInfo ?? Connection.ConnectionInfo;

			return new ConnectionInfo(StartTime, conn.Latency,
				conn.NumberOfNakReceive, conn.NumberOfAckReceive,
				conn.NumberOfAckSent, Session?.ErrorCount ?? 0,
				Session?.ResendCount ?? 0, conn?.TotalPacketSizeInPerSecond ?? 0, conn?.TotalPacketSizeOutPerSecond ?? 0);
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
			Session.SendPacket(packet);
		}

		[EventHandler(EventPriority.Highest)]
		private void OnSendChatMessage(ChatMessagePublishEvent e)
		{
			if (e.IsCancelled)
				return;

			var message = e.ChatObject.RawMessage;
			if (message[0] == '/')
			{
				McpeCommandRequest commandRequest = McpeCommandRequest.CreateObject();
				commandRequest.command = message;
				commandRequest.unknownUuid = new MiNET.Utils.UUID(Guid.NewGuid().ToString());
				Session.SendPacket(commandRequest);
			}
			else
			{
				SendChat(message);
			}
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

		private bool _disconnectShown = false;
		public void ShowDisconnect(string reason, bool useTranslation = false)
		{
			if (Alex.GameStateManager.GetActiveState() is DisconnectedScreen s)
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

            MessageHandler.CryptoContext = new CryptoContext() {ClientKey = clientKey, UseEncryption = false,};

            Session.SendPacket(loginPacket);
            
       /*     var packet = McpeClientCacheStatus.CreateObject();
            packet.enabled = false;
				
            Session.SendPacket(packet);*/

        //    Session.CryptoContext.UseEncryption = true;
        }

		public void SendMcpeMovePlayer(MiNET.Utils.PlayerLocation location, bool onGround)
		{
			var movePlayerPacket = McpeMovePlayer.CreateObject();
			movePlayerPacket.runtimeEntityId = EntityId;
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

		public new void InitiateEncryption(byte[] serverKey, byte[] randomKeyToken)
		{
			try
			{
				ECPublicKeyParameters remotePublicKey = (ECPublicKeyParameters)
					PublicKeyFactory.CreateKey(serverKey);

				var handler = MessageHandler;
				
				ECDHBasicAgreement agreement = new ECDHBasicAgreement();
				agreement.Init(handler.CryptoContext.ClientKey.Private);
				byte[] secret;
				using (var sha = SHA256.Create())
				{
					secret = sha.ComputeHash(randomKeyToken.Concat(agreement.CalculateAgreement(remotePublicKey).ToByteArrayUnsigned()).ToArray());
				}
		        
				// Create a decrytor to perform the stream transform.
				IBufferedCipher decryptor = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
				decryptor.Init(false, new ParametersWithIV(new KeyParameter(secret), secret.Take(16).ToArray()));

				IBufferedCipher encryptor = CipherUtilities.GetCipher("AES/CFB8/NoPadding");
				encryptor.Init(true, new ParametersWithIV(new KeyParameter(secret), secret.Take(16).ToArray()));

				handler.CryptoContext = new CryptoContext
				{
					Decryptor = decryptor,
					Encryptor = encryptor,
					UseEncryption = true,
					Key = secret,
					ClientKey = handler.CryptoContext.ClientKey
				};

				Thread.Sleep(1250);
				McpeClientToServerHandshake magic = new McpeClientToServerHandshake();
				Session.SendPacket(magic);
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

        private byte[] EncodeSkinJwt(AsymmetricCipherKeyPair newKey, ECDsa signKey, string username, string x5u)
        {
	        #region Skin Json

	        var skinText = "{\"Slim\":false,\"IsPersonaSkin\":false,\"IsPremiumSkin\":false,\"Cape\":{\"Id\":\"\",\"ImageHeight\":0,\"ImageWidth\":0,\"Data\":\"\",\"OnClassicSkin\":false},\"SkinId\":\"geometry.humanoid.customSlim\",\"ResourcePatch\":\"{\\n   \\\"geometry\\\" : {\\n      \\\"default\\\" : \\\"geometry.humanoid.customSlim\\\"\\n   }\\n}\\n\",\"SkinResourcePatch\":{\"Geometry\":{\"Default\":\"geometry.humanoid.customSlim\"}},\"Height\":32,\"Width\":64,\"Data\":\"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\",\"GeometryName\":null,\"GeometryData\":\"{\\\\\\\"format_version\\\\\\\":\\\\\\\"1.14.0\\\\\\\",\\\\\\\"minecraft:geometry\\\\\\\":[{\\\\\\\"bones\\\\\\\":[{\\\\\\\"name\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"waist\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"waist\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-5.0,8.0,3.0],\\\\\\\"size\\\\\\\":[10,16,1],\\\\\\\"uv\\\\\\\":[0,0]}],\\\\\\\"name\\\\\\\":\\\\\\\"cape\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,3.0],\\\\\\\"rotation\\\\\\\":[0.0,180.0,0.0]}],\\\\\\\"description\\\\\\\":{\\\\\\\"identifier\\\\\\\":\\\\\\\"geometry.cape\\\\\\\",\\\\\\\"texture_height\\\\\\\":32,\\\\\\\"texture_width\\\\\\\":64}},{\\\\\\\"bones\\\\\\\":[{\\\\\\\"name\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,0.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-4.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[8,12,4],\\\\\\\"uv\\\\\\\":[16,16]}],\\\\\\\"name\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"waist\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"waist\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-4.0,24.0,-4.0],\\\\\\\"size\\\\\\\":[8,8,8],\\\\\\\"uv\\\\\\\":[0,0]}],\\\\\\\"name\\\\\\\":\\\\\\\"head\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"cape\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24,3.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.50,\\\\\\\"origin\\\\\\\":[-4.0,24.0,-4.0],\\\\\\\"size\\\\\\\":[8,8,8],\\\\\\\"uv\\\\\\\":[32,0]}],\\\\\\\"name\\\\\\\":\\\\\\\"hat\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"head\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[4.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[32,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftArm\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[5.0,22.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[4.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[48,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftSleeve\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"leftArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[5.0,22.0,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"leftItem\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"leftArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[6.0,15.0,1.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-8.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[40,16]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightArm\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[-5.0,22.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-8.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[40,32]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightSleeve\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"rightArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[-5.0,22.0,0.0]},{\\\\\\\"locators\\\\\\\":{\\\\\\\"lead_hold\\\\\\\":[-6,15,1]},\\\\\\\"name\\\\\\\":\\\\\\\"rightItem\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"rightArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[-6,15,1]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-0.10,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[16,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftLeg\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-0.10,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftPants\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"leftLeg\\\\\\\",\\\\\\\"pivot\\\\\\\":[1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-3.90,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,16]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightLeg\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[-1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-3.90,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,32]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightPants\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"rightLeg\\\\\\\",\\\\\\\"pivot\\\\\\\":[-1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-4.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[8,12,4],\\\\\\\"uv\\\\\\\":[16,32]}],\\\\\\\"name\\\\\\\":\\\\\\\"jacket\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]}],\\\\\\\"description\\\\\\\":{\\\\\\\"identifier\\\\\\\":\\\\\\\"geometry.humanoid.custom\\\\\\\",\\\\\\\"texture_height\\\\\\\":64,\\\\\\\"texture_width\\\\\\\":64,\\\\\\\"visible_bounds_height\\\\\\\":2,\\\\\\\"visible_bounds_offset\\\\\\\":[0,1,0],\\\\\\\"visible_bounds_width\\\\\\\":1}},{\\\\\\\"bones\\\\\\\":[{\\\\\\\"name\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,0.0,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"waist\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-4.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[8,12,4],\\\\\\\"uv\\\\\\\":[16,16]}],\\\\\\\"name\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"waist\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-4.0,24.0,-4.0],\\\\\\\"size\\\\\\\":[8,8,8],\\\\\\\"uv\\\\\\\":[0,0]}],\\\\\\\"name\\\\\\\":\\\\\\\"head\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.50,\\\\\\\"origin\\\\\\\":[-4.0,24.0,-4.0],\\\\\\\"size\\\\\\\":[8,8,8],\\\\\\\"uv\\\\\\\":[32,0]}],\\\\\\\"name\\\\\\\":\\\\\\\"hat\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"head\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-3.90,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,16]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightLeg\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[-1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-3.90,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,32]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightPants\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"rightLeg\\\\\\\",\\\\\\\"pivot\\\\\\\":[-1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-0.10,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,16]}],\\\\\\\"mirror\\\\\\\":true,\\\\\\\"name\\\\\\\":\\\\\\\"leftLeg\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"root\\\\\\\",\\\\\\\"pivot\\\\\\\":[1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-0.10,0.0,-2.0],\\\\\\\"size\\\\\\\":[4,12,4],\\\\\\\"uv\\\\\\\":[0,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftPants\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"leftLeg\\\\\\\",\\\\\\\"pivot\\\\\\\":[1.90,12.0,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[4.0,11.50,-2.0],\\\\\\\"size\\\\\\\":[3,12,4],\\\\\\\"uv\\\\\\\":[32,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftArm\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[5.0,21.50,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[4.0,11.50,-2.0],\\\\\\\"size\\\\\\\":[3,12,4],\\\\\\\"uv\\\\\\\":[48,48]}],\\\\\\\"name\\\\\\\":\\\\\\\"leftSleeve\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"leftArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[5.0,21.50,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"leftItem\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"leftArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[6,14.50,1]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"origin\\\\\\\":[-7.0,11.50,-2.0],\\\\\\\"size\\\\\\\":[3,12,4],\\\\\\\"uv\\\\\\\":[40,16]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightArm\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[-5.0,21.50,0.0]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-7.0,11.50,-2.0],\\\\\\\"size\\\\\\\":[3,12,4],\\\\\\\"uv\\\\\\\":[40,32]}],\\\\\\\"name\\\\\\\":\\\\\\\"rightSleeve\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"rightArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[-5.0,21.50,0.0]},{\\\\\\\"locators\\\\\\\":{\\\\\\\"lead_hold\\\\\\\":[-6,14.50,1]},\\\\\\\"name\\\\\\\":\\\\\\\"rightItem\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"rightArm\\\\\\\",\\\\\\\"pivot\\\\\\\":[-6,14.50,1]},{\\\\\\\"cubes\\\\\\\":[{\\\\\\\"inflate\\\\\\\":0.250,\\\\\\\"origin\\\\\\\":[-4.0,12.0,-2.0],\\\\\\\"size\\\\\\\":[8,12,4],\\\\\\\"uv\\\\\\\":[16,32]}],\\\\\\\"name\\\\\\\":\\\\\\\"jacket\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24.0,0.0]},{\\\\\\\"name\\\\\\\":\\\\\\\"cape\\\\\\\",\\\\\\\"parent\\\\\\\":\\\\\\\"body\\\\\\\",\\\\\\\"pivot\\\\\\\":[0.0,24,-3.0]}],\\\\\\\"description\\\\\\\":{\\\\\\\"identifier\\\\\\\":\\\\\\\"geometry.humanoid.customSlim\\\\\\\",\\\\\\\"texture_height\\\\\\\":64,\\\\\\\"texture_width\\\\\\\":64,\\\\\\\"visible_bounds_height\\\\\\\":2,\\\\\\\"visible_bounds_offset\\\\\\\":[0,1,0],\\\\\\\"visible_bounds_width\\\\\\\":1}}]}\",\"AnimationData\":\"\",\"Animations\":[]}";
	        
	        #endregion
	        
	        Skin skin;

	     /*   if (Alex.PlayerModel != null && Alex.PlayerTexture != null)
	        {
		        var model = Alex.PlayerModel;

		        byte[] skinData;
		        using (MemoryStream ms = new MemoryStream())
		        {
			        Alex.PlayerTexture.SaveAsPng(ms, new PngEncoder());
			        skinData = ms.ToArray();
		        }
		        
		        skin = new Skin()
		        {
			        Cape = new Cape()
			        {
				       Data = new byte[0]
			        },
			        SkinId = model.Name,
			        ResourcePatch =
				        Convert.ToBase64String(
					        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SkinResourcePatch()
						        {
							        Geometry = new GeometryIdentifier()
							        {
								        Default = model.Name
							        }
						        }
					        ))),
			        Width = (int) model.Texturewidth,
			        Height = (int) model.Textureheight,
			        Data = skinData,
			        GeometryName = model.Name,
			        GeometryData =
				        Convert.ToBase64String(
					        Encoding.UTF8.GetBytes(MCJsonConvert.SerializeObject(model))),
			        //AnimationData = ,
			        IsPremiumSkin = false,
			        IsPersonaSkin = false,
		        };
	        }
	        else
	       {*/
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
	    //    }

	        string val = JWT.Encode(JsonConvert.SerializeObject(new BedrockSkinData(skin)
            {
	            ClientRandomId = new Random().Next(),
	            LanguageCode = Alex.Services.GetService<IOptionsProvider>().AlexOptions.MiscelaneousOptions.Language.Value,
	            ServerAddress = $"{ServerEndpoint.Address.ToString()}:{ServerEndpoint.Port.ToString()}",
	             ThirdPartyName = username,
	             DeviceId = Alex.Resources.DeviceID
            }, new JsonSerializerSettings()
	        {
		        ContractResolver = new DefaultContractResolver
		        {
			        NamingStrategy = new DefaultNamingStrategy(){}
		        },
		        DefaultValueHandling = DefaultValueHandling.Include
	        }), signKey, JwsAlgorithm.ES384, new Dictionary<string, object> { { "x5u", x5u } }, new JwtSettings()
            {
                JsonMapper = new JWTMapper()
            });

            return Encoding.UTF8.GetBytes(val);
        }

        /*public bool IgnoreUnConnectedPong = false;
		public override void OnUnconnectedPong(UnconnectedPong packet, IPEndPoint senderEndpoint)
		{
			KnownMotd = new BedrockMotd(packet.serverName);
			OnMotdReceivedHandler?.Invoke(this, KnownMotd);
			if (IgnoreUnConnectedPong) return;

			base.OnUnconnectedPong(packet, senderEndpoint);
		}*/
		
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
			
			Session.SendPacket(text);
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
			
			Session.SendPacket(packet);
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
                        Face = (int) face,
                        Slot = player.Inventory.SelectedSlot,
                        //Item = item.
                        Item = MiNET.Items.ItemFactory.GetItem(item.Id, item.Meta, item.Count)
                        
                    };

                   Session.SendPacket(packet);
                }
                else if (status == DiggingStatus.Cancelled)
                {
                    SendPlayerAction(PlayerAction.AbortBreak, position, (int) face);
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

	    public override void BlockPlaced(BlockCoordinates position, API.Blocks.BlockFace face, int hand, Vector3 cursorPosition, Entity entity)
	    {
		    if (entity is Player p)
		    {
			    var itemInHand = p.Inventory[hand];
			    var minetItem = GetMiNETItem(itemInHand);
			    
			    Log.Info($"Placing block, slot={hand} InHand={itemInHand.ToString()} face={face} pos={position}");
			    
			    var packet = McpeInventoryTransaction.CreateObject();
			    packet.transaction = new ItemUseTransaction()
			    {
				    ActionType = (int) McpeInventoryTransaction.ItemUseAction.Place,
				    ClickPosition =
					    new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
				    //TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
				    // = NetworkEntityId,
				    Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
				    Face = (int) face,
				    TransactionRecords = new List<TransactionRecord>()
				    {

				    },
				    Item = minetItem,
				    FromPosition = new System.Numerics.Vector3(p.KnownPosition.X, p.KnownPosition.Y, p.KnownPosition.Z),
				    Slot = hand,
				    BlockRuntimeId = ChunkProcessor._blockStateMap.FirstOrDefault(x => x.Value.Id == itemInHand.Id && x.Value.Data == itemInHand.Meta).Key
				    //BlockRuntimeId = 
			    };

			    Session.SendPacket(packet);
		    }
	    }

		public override void EntityInteraction(Entity player, Entity target,
		    ItemUseOnEntityAction action, int hand)
	    {
		    if (player is Player p)
		    {
			    var itemInHand = p.Inventory[p.Inventory.SelectedSlot];

			    // WorldProvider?.GetChatReceiver?.Receive(new ChatObject($"(CLIENT) Hit entity: {target.EntityId} | Action: {action.ToString()} | Item: {itemInHand.Id}:{itemInHand.Meta} ({itemInHand.Name})"));

			    var item = GetMiNETItem(itemInHand); //MiNET.Items.ItemFactory.GetItem(itemInHand.Id, itemInHand.Meta, itemInHand.Count);
			  //  item.Metadata = itemInHand.Meta;
			  //  item.ExtraData = itemInHand.Nbt;
			   // item.Count = (byte) itemInHand.Count;
			    
			    var packet = McpeInventoryTransaction.CreateObject();
			    packet.transaction = new ItemUseOnEntityTransaction()
			    {
				    ActionType = (McpeInventoryTransaction.ItemUseOnEntityAction) action,
				    Item = item,
				    EntityId = target.EntityId
			    };
			    /*  packet.transaction = new Transaction()
		    {
			    TransactionType = McpeInventoryTransaction.TransactionType.ItemUseOnEntity,
			    ActionType = (int) action,
			    Item = MiNET.Items.ItemFactory.GetItem(itemInHand.Id, itemInHand.Meta, itemInHand.Count),
			    EntityId = target.EntityId
		    };*/

			    Session.SendPacket(packet);
		    }
	    }

	    public override void WorldInteraction(BlockCoordinates position, API.Blocks.BlockFace face, int hand, Vector3 cursorPosition)
	    {
		    var packet = McpeInventoryTransaction.CreateObject();
		    packet.transaction = new ItemUseTransaction()
		    {
			    ActionType = McpeInventoryTransaction.ItemUseAction.Clickblock,
			    ClickPosition =
				    new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
			  //  TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
			   // EntityId = NetworkEntityId,
			    Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
			    Face = (int)face,
		    };
		  /*  packet.transaction = new Transaction()
		    {
			    ActionType = (int)McpeInventoryTransaction.ItemUseAction.Use,
			    ClickPosition =
				    new System.Numerics.Vector3(cursorPosition.X, cursorPosition.Y, cursorPosition.Z),
			    TransactionType = McpeInventoryTransaction.TransactionType.ItemUse,
			    EntityId = NetworkEntityId,
			    Position = new MiNET.Utils.BlockCoordinates(position.X, position.Y, position.Z),
			    Face = (int)face,
                
			    //Item = MiNET.Items.ItemFactory.GetItem()

		    };*/

		  Session.SendPacket(packet);
	    }

	    public override void UseItem(Item item, int hand, ItemUseAction action)
	    {
		    var minetItem = GetMiNETItem(item);
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
		    packet.transaction = new ItemUseTransaction()
		    {
			    ActionType = useAction,
			    Item = minetItem
		    };

		    Session.SendPacket(packet);
		    
			//Log.Warn("TODO: Implement UseItem");
		}

		public override void HeldItemChanged(Item item, short slot)
		{
			var minetItem = GetMiNETItem(item);
			
			McpeMobEquipment packet = new MobEquipment();
			packet.selectedSlot = (byte) slot;
			packet.slot = (byte) slot;
			packet.item = minetItem;
			
			Session.SendPacket(packet);
			
			Log.Warn($"Held item slot changed: {slot} | Inventor: ");
		}

		public override void Close()
		{
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
					Connection.ConnectionInfo.ThroughPut.Change(Timeout.Infinite, Timeout.Infinite);
				}
			});

			//Task.Delay(500).ContinueWith(task => { base.StopClient(); });
		}

		public void RequestChunkRadius(int radius)
		{
			var packet = McpeRequestChunkRadius.CreateObject();
			packet.chunkRadius = radius;

			Session?.SendPacket(packet);
		}

		public void SendDisconnectionNotification()
		{
			Session?.SendPacket(new DisconnectionNotification());
		}

		public void SendPing()
		{
			ConnectedPing cp = ConnectedPing.CreateObject();
			cp.sendpingtime = DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

			CustomConnectedPong.LastSentPing = DateTimeOffset.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
			
			Session?.SendPacket(cp);
		}


		public void Dispose()
		{
			EventDispatcher?.UnregisterEvents(this);
			
			Close();
			_threadPool.Dispose();
			//_threadPool.WaitForThreadsExit();
		}

		private CommandSet _availableCommandSet;
		public void LoadCommands(CommandSet commandSet)
		{
			_availableCommandSet = commandSet;
			/*ThreadPool.QueueUserWorkItem(o =>
			{
				Debug.WriteLine($"Saving commands!");
				File.WriteAllText("commands.json", JsonConvert.SerializeObject(commandSet, Formatting.Indented));
				//Log.Info($"Commands: {JsonConvert.SerializeObject(commandSet, Formatting.Indented)}");
			});*/
		}
	}
}
