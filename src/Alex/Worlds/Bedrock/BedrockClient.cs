using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data;
using Alex.API.Data.Options;
using Alex.API.Entities;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Network;
using Alex.API.Network.Bedrock;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Gamestates;
using Alex.Services;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Items;
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
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using LevelInfo = MiNET.Worlds.LevelInfo;
using NewtonsoftMapper = MiNET.NewtonsoftMapper;
using Player = Alex.Entities.Player;
using PlayerLocation = MiNET.Utils.PlayerLocation;
using Skin = MiNET.Utils.Skins.Skin;

namespace Alex.Worlds.Bedrock
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
	public class BedrockClient : IBedrockNetworkProvider, IDisposable
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClient));
		
		private ManualResetEventSlim ConnectionAcceptedWaitHandle { get; set; }
		public BedrockWorldProvider WorldProvider { get; }
		public EventHandler<BedrockMotd> OnMotdReceivedHandler;
		//public BedrockMotd KnownMotd = new BedrockMotd(string.Empty);

        private Alex Alex { get; }
        private IOptionsProvider OptionsProvider { get; }
        private XBLMSAService XblmsaService { get; }
        private AlexOptions Options => OptionsProvider.AlexOptions;
        private DedicatedThreadPool _threadPool;
        
        public PlayerProfile PlayerProfile { get; }
        private CancellationTokenSource CancellationTokenSource { get; }
        
        public McpeNetworkChunkPublisherUpdate LastChunkPublish { get; set; }
        public bool HasSpawned { get; set; }
        public AutoResetEvent PlayerStatusChanged { get; set; } = new AutoResetEvent(false);
        private IEventDispatcher EventDispatcher { get; }
        public RakConnection Connection { get; }
        private BedrockClientMessageHandler MessageHandler { get; set; }
        public RakSession Session => Connection.ConnectionInfo.RakSessions.Values.FirstOrDefault();
        public bool IsConnected => Session?.State == ConnectionState.Connected;
        private IPEndPoint ServerEndpoint { get; }
        private long ClientGUID { get; }
		public BedrockClient(Alex alex, IEventDispatcher eventDispatcher, IPEndPoint endpoint, PlayerProfile playerProfile, DedicatedThreadPool threadPool, BedrockWorldProvider wp)
		{
			ServerEndpoint = endpoint;
			
			Connection = new RakConnection(new GreyListManager(), new MotdProvider(), threadPool);

			Connection.RemoteEndpoint = endpoint;
			PlayerProfile = playerProfile;
			CancellationTokenSource = new CancellationTokenSource();
			
            Alex = alex;
			WorldProvider = wp;
			//ConnectionAcceptedWaitHandle = new ManualResetEventSlim(false);
			//MessageDispatcher = new McpeClientMessageDispatcher(new BedrockClientPacketHandler(this, eventDispatcher, wp, playerProfile, alex, CancellationTokenSource.Token));
			//CurrentLocation = new MiNET.Utils.PlayerLocation(0,0,0);
			OptionsProvider = alex.Services.GetRequiredService<IOptionsProvider>();
			XblmsaService = alex.Services.GetRequiredService<XBLMSAService>();
			
		//	base.ChunkRadius = Options.VideoOptions.RenderDistance;
			
			Options.VideoOptions.RenderDistance.Bind(RenderDistanceChanged);

			_threadPool = threadPool;
			//Log.IsDebugEnabled = false;
			//this.RegisterEventHandlers();
			
			MessageHandler = new BedrockClientMessageHandler(Session, new BedrockClientPacketHandler(this, eventDispatcher, wp, playerProfile, alex, CancellationTokenSource.Token));
			MessageHandler.ConnectionAction = () =>
			{
				ConnectionAcceptedWaitHandle?.Set();
				SendAlexLogin(playerProfile.Username);
			};
			Connection.CustomMessageHandlerFactory = session => MessageHandler;
			
			eventDispatcher?.RegisterEvents(this);

			EventDispatcher = eventDispatcher;
			
			byte[] buffer = new byte[8];
			new Random().NextBytes(buffer);
			ClientGUID = BitConverter.ToInt64(buffer, 0);
		}

		public void Start(ManualResetEventSlim resetEvent)
		{
			Connection.Start();
			//StartClient();
			//HaveServer = true;

			if (Connection.AutoConnect)
			{
			//	Connection._rakOfflineHandler.HaveServer = true;
			}

			ConnectionAcceptedWaitHandle = resetEvent;
			
			//SendUnconnectedPing();

		//	if (!Connection.AutoConnect)
			//{
				ThreadPool.QueueUserWorkItem(o =>
				{
					Stopwatch sw = Stopwatch.StartNew();
					while (sw.ElapsedMilliseconds < 15000)
					{
						if (!Connection.AutoConnect)
						{
							SendUnconnectedPing();
							Thread.Sleep(500);
						}
						
						if (!Connection.AutoConnect && !string.IsNullOrWhiteSpace(Connection.RemoteServerName))
						{
							OnMotdReceivedHandler?.Invoke(this, new BedrockMotd(Connection.RemoteServerName)
							{
								ServerEndpoint = Connection.RemoteEndpoint
							});
							resetEvent.Set();
							break;
						}

						if (Connection.AutoConnect && (IsConnected))
						{
							resetEvent.Set();
							break;
						}
					}
				});

				if (Connection.AutoConnect)
				{
					SendOpenConnectionRequest1();
				}
				else
				{
					
				}
				//}

		}
		
		private void SendData(byte[] data, IPEndPoint targetEndpoint)
		{
			if (Connection == null) return;

			try
			{
				Connection.SendData(data, targetEndpoint);
			}
			catch (Exception e)
			{
				Log.Debug("Send exception", e);
			}
		}

		public void SendUnconnectedPing()
		{
			var packet = new UnconnectedPing
			{
				pingId = Stopwatch.GetTimestamp() /*incoming.pingId*/,
				guid = ClientGUID
			};

			var data = packet.Encode();

			if (ServerEndpoint != null)
			{
				SendData(data, ServerEndpoint);
			}
			else
			{
				SendData(data, new IPEndPoint(IPAddress.Broadcast, 19132));
			}
		}

		public void SendConnectedPing()
		{
			var packet = new ConnectedPing() {sendpingtime = DateTime.UtcNow.Ticks};

			SendPacket(packet);
		}

		public void SendConnectedPong(long sendpingtime)
		{
			var packet = new ConnectedPong
			{
				sendpingtime = sendpingtime,
				sendpongtime = sendpingtime + 200
			};

			SendPacket(packet);
		}

		public void SendOpenConnectionRequest1()
		{
			Connection._rakOfflineHandler.SendOpenConnectionRequest1(ServerEndpoint);
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
		private void SendAlexLogin(string username)
        {
            JWT.JsonMapper = new NewtonsoftMapper();

            var clientKey = XblmsaService.MinecraftKeyPair;// CryptoUtils.GenerateClientKey();

            ECDsa signKey = ConvertToSingKeyFormat(clientKey);

            string b64Key = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(clientKey.Public).GetEncoded().EncodeBase64();

            string identity, xuid = "";
			byte[] certChain = null;

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

            var skinData = EncodeSkinJwt(clientKey, signKey, username, b64Key);

	        byte[] data = CryptoUtils.CompressJwtBytes(certChain, skinData, CompressionLevel.Fastest);

            McpeLogin loginPacket = new McpeLogin
            {
                protocolVersion = McpeProtocolInfo.ProtocolVersion,
                payload = data
            };

          /*  Session.CryptoContext = new CryptoContext()
            {
	            ClientKey = clientKey,
	            UseEncryption = false,
            };
*/
            Session.SendPacket(loginPacket);

        //    Session.CryptoContext.UseEncryption = true;
        }

		public void SendMcpeMovePlayer()
		{
			if (CurrentLocation == null) return;

			if (CurrentLocation.Y < 0)
				CurrentLocation.Y = 64f;

			var movePlayerPacket = McpeMovePlayer.CreateObject();
			movePlayerPacket.runtimeEntityId = EntityId;
			movePlayerPacket.x = CurrentLocation.X;
			movePlayerPacket.y = CurrentLocation.Y;
			movePlayerPacket.z = CurrentLocation.Z;
			movePlayerPacket.yaw = CurrentLocation.Yaw;
			movePlayerPacket.pitch = CurrentLocation.Pitch;
			movePlayerPacket.headYaw = CurrentLocation.HeadYaw;
			movePlayerPacket.mode = 1;
			movePlayerPacket.onGround = false;

			SendPacket(movePlayerPacket);
		}

		public new void InitiateEncryption(byte[] serverKey, byte[] randomKeyToken)
		{
			try
			{
				ECPublicKeyParameters remotePublicKey = (ECPublicKeyParameters)
					PublicKeyFactory.CreateKey(serverKey);
				
				ECDHBasicAgreement agreement = new ECDHBasicAgreement();
				agreement.Init(MessageHandler.CryptoContext.ClientKey.Private);
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

				/*Session.CryptoContext = new CryptoContext
				{
					Decryptor = decryptor,
					Encryptor = encryptor,
					UseEncryption = true,
					Key = secret
				};*/
				MessageHandler.CryptoContext.Decryptor = decryptor;
				MessageHandler.CryptoContext.Encryptor = encryptor;
				MessageHandler.CryptoContext.Key = secret;
				MessageHandler.CryptoContext.UseEncryption = true;

				//Thread.Sleep(1250);
				McpeClientToServerHandshake magic = new McpeClientToServerHandshake();
				Session.SendPacket(magic);
			}
			catch (Exception e)
			{
				Log.Error("Initiate encryption", e);
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

        private byte[] EncodeSkinJwt(AsymmetricCipherKeyPair newKey, ECDsa signKey, string username, string x5u)
        {
	        var skinText = File.ReadAllText("skin.json");
	        dynamic payload = JObject.Parse(skinText);
	        
	        var skin = new Skin()
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
		        ResourcePatch = Convert.ToBase64String(Encoding.UTF8.GetBytes((string) payload.ResourcePatch ?? string.Empty)),
		        Width = payload.Width,
		        Height = payload.Height,
		        Data = Convert.FromBase64String((string) payload.Data ?? string.Empty),
		        GeometryData = Convert.ToBase64String(Encoding.UTF8.GetBytes((string) payload.SkinGeometryData ?? string.Empty)),
		        AnimationData = payload.AnimationData,
		        IsPremiumSkin = payload.IsPremiumSkin,
		        IsPersonaSkin = payload.IsPersonaSkin,
	        };

	        string val = JWT.Encode(JsonConvert.SerializeObject(new BedrockSkinData(skin)
            {
	            ClientRandomId = new Random().Next(),
	            LanguageCode = "en_US",
	            ServerAddress = $"{ServerEndpoint.Address.ToString()}:{ServerEndpoint.Port.ToString()}",
	             ThirdPartyName = username
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
		
		public IWorldReceiver WorldReceiver { get; set; } 
		public System.Numerics.Vector3 SpawnPoint { get; set; } = System.Numerics.Vector3.Zero;
		public LevelInfo LevelInfo { get; } = new LevelInfo();
		public PlayerLocation CurrentLocation { get; set; } = new PlayerLocation();

		void INetworkProvider.EntityAction(int entityId, EntityAction action)
		{
			PlayerAction translated;
			switch (action)
			{
				case EntityAction.StartSneaking:
					translated = PlayerAction.StartSneak;
					break;
				case EntityAction.StopSneaking:
					translated = PlayerAction.StopSneak;
					break;

				case EntityAction.StartSprinting:
					translated = PlayerAction.StartSprint;
					break;
				case EntityAction.StopSprinting:
					translated = PlayerAction.StopSprint;
					break;

				default:
					return;
			}
			
			SendPlayerAction(translated, null, null);
		}

		private void SendChat(string message)
		{
			McpeText text = McpeText.CreateObject();
			text.message = message;
			Session.SendPacket(text);
		}

		void INetworkProvider.SendChatMessage(string message)
		{
			SendChat(message);
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
		
	    public void PlayerDigging(DiggingStatus status, BlockCoordinates position, BlockFace face, Vector3 cursorPosition)
	    {
            if (WorldReceiver?.GetPlayerEntity() is Entities.Player player)
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

	    public void BlockPlaced(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition, IEntity entity)
	    {
		    if (entity is Player p)
		    {
			    var itemInHand = p.Inventory[hand];
			    var minetItem = MiNET.Items.ItemFactory.GetItem(itemInHand.Id, itemInHand.Meta, itemInHand.Count);
			    minetItem.ExtraData = itemInHand.Nbt;
			    
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
				    Slot = hand
				    //BlockRuntimeId = 
			    };

			    Session.SendPacket(packet);
		    }
	    }

		    public void EntityInteraction(IEntity player, IEntity target,
		    McpeInventoryTransaction.ItemUseOnEntityAction action)
	    {
		    if (player is Player p)
		    {
			    var itemInHand = p.Inventory[p.Inventory.SelectedSlot];
			    
			   // WorldProvider?.GetChatReceiver?.Receive(new ChatObject($"(CLIENT) Hit entity: {target.EntityId} | Action: {action.ToString()} | Item: {itemInHand.Id}:{itemInHand.Meta} ({itemInHand.Name})"));
			    
			    var packet = McpeInventoryTransaction.CreateObject();
			    packet.transaction = new ItemUseOnEntityTransaction()
			    {
				    ActionType = action,
				    Item = MiNET.Items.ItemFactory.GetItem(itemInHand.Id, itemInHand.Meta, itemInHand.Count),
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

	    public void WorldInteraction(BlockCoordinates position, BlockFace face, int hand, Vector3 cursorPosition)
	    {
		    var packet = McpeInventoryTransaction.CreateObject();
		    packet.transaction = new ItemUseTransaction()
		    {
			    ActionType = McpeInventoryTransaction.ItemUseAction.Use,
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

	    public void UseItem(int hand)
		{
			Log.Warn("TODO: Implement UseItem");
		}

		public void HeldItemChanged(short slot)
		{
			Log.Warn("TODO: Implement Held Item Changed");
		}

		public void Close()
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
			});

			//Task.Delay(500).ContinueWith(task => { base.StopClient(); });
		}

		public void RequestChunkRadius(int radius)
		{
			var packet = McpeRequestChunkRadius.CreateObject();
			packet.chunkRadius = radius;

			Session.SendPacket(packet);
		}

		public void SendDisconnectionNotification()
		{
			Session?.SendPacket(new DisconnectionNotification());
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
