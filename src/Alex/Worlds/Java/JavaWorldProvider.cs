using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data;
using Alex.API.Data.Options;
using Alex.API.Entities;
using Alex.API.Events;
using Alex.API.Events.World;
using Alex.API.Graphics;
using Alex.API.Network;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Entities;
using Alex.Gamestates;
using Alex.Graphics.Models.Entity;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java;
using Alex.Networking.Java.Events;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Login;
using Alex.Networking.Java.Packets.Play;
using Alex.Networking.Java.Util;
using Alex.Networking.Java.Util.Encryption;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Utils;
using Newtonsoft.Json;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using LevelInfo = Alex.API.World.LevelInfo;
using NibbleArray = Alex.API.Utils.NibbleArray;
using Packet = Alex.Networking.Java.Packets.Packet;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Worlds.Java
{
	internal interface IJavaProvider
	{
		void HandleHandshake(Packet packet);
		void HandleStatus(Packet packet);
		void HandleLogin(Packet packet);
		void HandlePlay(Packet packet);
	}
	public class JavaWorldProvider : WorldProvider, IJavaProvider
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private Alex Alex { get; }
		private JavaClient Client { get; }
		private PlayerProfile Profile { get; }
		
		private IOptionsProvider OptionsProvider { get; }
		private AlexOptions Options => OptionsProvider.AlexOptions;

		private IPEndPoint Endpoint;
		private ManualResetEvent _loginCompleteEvent = new ManualResetEvent(false);
		private TcpClient TcpClient;

		private System.Threading.Timer _gameTickTimer;
		private DedicatedThreadPool ThreadPool;
		private IEventDispatcher EventDispatcher { get; }
		public string Hostname { get; set; }
		
		private JavaNetworkProvider NetworkProvider { get; }
		public JavaWorldProvider(Alex alex, IPEndPoint endPoint, PlayerProfile profile, out NetworkProvider networkProvider)
		{
			Alex = alex;
			Profile = profile;
			Endpoint = endPoint;
			
			OptionsProvider = alex.Services.GetRequiredService<IOptionsProvider>();
			EventDispatcher = alex.Services.GetRequiredService<IEventDispatcher>();
			
			ThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount));

			TcpClient = new TcpClient();
			Client = new JavaClient(this, TcpClient.Client);
			Client.OnConnectionClosed += OnConnectionClosed;
			
			NetworkProvider = new JavaNetworkProvider(Client);;
			networkProvider = NetworkProvider;
			
			EventDispatcher.RegisterEvents(this);
		}

		private bool _disconnected = false;
		private string _disconnectReason = string.Empty;

		private void OnConnectionClosed(object sender, ConnectionClosedEventArgs e)
		{
			if (_disconnected) return;
			_disconnected = true;

			if (e.Graceful)
			{
				ShowDisconnect("You've been disconnected!");
			}
			else
			{
				ShowDisconnect("disconnect.closed", true);
			}

			_loginCompleteEvent.Set();
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

		private PlayerLocation _lastSentLocation = new PlayerLocation(Vector3.Zero);
		private int _tickSinceLastPositionUpdate = 0;
		private bool _flying = false;

		private void SendPlayerAbilities(Player player)
		{
			int flags = 0;

			if (_flying)
			{
				flags |= 0x01 << flags;
			}

			if (player.CanFly)
			{
				flags |= 0x03 << flags;
			}

			PlayerAbilitiesPacket abilitiesPacket = new PlayerAbilitiesPacket();
			abilitiesPacket.ServerBound = true;

			abilitiesPacket.Flags = (byte) flags;
			abilitiesPacket.FlyingSpeed = (float) player.FlyingSpeed;
			abilitiesPacket.WalkingSpeed = (float)player.MovementSpeed;

			SendPacket(abilitiesPacket);
		}

		private object _entityTicks = new object();
		private bool _isSneaking = false;
		private bool _isSprinting = false;
		private void GameTick(object state)
		{
			if (World == null) return;

			if (_initiated)
			{
				var p = World.Player;
				if (p != null && p is Player player && Spawned && !Respawning)
				{
					player.IsSpawned = Spawned;

					if (player.IsFlying != _flying)
					{
						_flying = player.IsFlying;

						SendPlayerAbilities(player);
					}

					var pos = (PlayerLocation)player.KnownPosition.Clone();
					if (pos.DistanceTo(_lastSentLocation) > 0.0f)
					{
						PlayerPositionAndLookPacketServerBound packet = new PlayerPositionAndLookPacketServerBound();
						packet.Yaw = pos.HeadYaw;
						packet.Pitch = -pos.Pitch;
						packet.X = pos.X;
						packet.Y = pos.Y;
						packet.Z = pos.Z;
						packet.OnGround = pos.OnGround;

						SendPacket(packet);
						_lastSentLocation = pos;

						_tickSinceLastPositionUpdate = 0;
						
						World.ChunkManager.FlagPrioritization();
					}
					else if (Math.Abs(pos.Pitch - _lastSentLocation.Pitch) > 0f || Math.Abs(pos.HeadYaw - _lastSentLocation.Yaw) > 0f)
					{
						PlayerLookPacket playerLook = new PlayerLookPacket();
						playerLook.Pitch = -pos.Pitch;
						playerLook.Yaw = pos.HeadYaw;
						playerLook.OnGround = pos.OnGround;

						SendPacket(playerLook);

						_tickSinceLastPositionUpdate = 0;
						
						World.ChunkManager.FlagPrioritization();
					}
					else if (_tickSinceLastPositionUpdate >= 20)
					{
						PlayerPosition packet = new PlayerPosition();
						packet.FeetY = pos.Y;
						packet.X = pos.X;
						packet.Z = pos.Z;
						packet.OnGround = pos.OnGround;

						SendPacket(packet);
						_lastSentLocation = pos;

						_tickSinceLastPositionUpdate = 0;
					}
					else
					{
						_tickSinceLastPositionUpdate++;
					}
				}
				
				p?.OnTick();
			}
		}

		private Vector3 _spawn = Vector3.Zero;
		public override Vector3 GetSpawnPoint()
		{
			return _spawn;
		}

		protected override void Initiate(out LevelInfo info)
		{
			info = new LevelInfo();

			_initiated = true;

			World?.UpdatePlayerPosition(_lastReceivedLocation);

			_gameTickTimer = new System.Threading.Timer(GameTick, null, 50, 50);
		}

		[EventHandler(EventPriority.Highest)]
		private void OnPublishChatMessage(ChatMessagePublishEvent e)
		{
			if (e.IsCancelled)
				return;
			
			NetworkProvider.SendChatMessage(e.ChatObject.RawMessage);
		}

		private int _transactionIds = 0;
		/*void IChatProvider.RequestTabComplete(string text, out int transactionId)
		{
			/*transactionId = Interlocked.Increment(ref _transactionIds);
			SendPacket(new TabCompleteServerBound()
			{
				Text = text,
				TransactionId = transactionId
			});*
		}*/

		private bool hasDoneInitialChunks = false;
		private bool _initiated = false;
		private BlockingCollection<ChunkColumn> _generatingHelper = new BlockingCollection<ChunkColumn>();
		private int _chunksReceived = 0;
		public override Task Load(ProgressReport progressReport)
		{
			return Task.Run(() =>
			{
				progressReport(LoadingState.ConnectingToServer, 0);
				if (!Login(Profile.PlayerName, Profile.Uuid, Profile.AccessToken))
				{
					_disconnected = true;
					ShowDisconnect("multiplayer.status.cannot_connect", true);
				}
				if (_disconnected) return;

				progressReport(LoadingState.ConnectingToServer, 99);

				_loginCompleteEvent.WaitOne();
				if (_disconnected) return;

				progressReport(LoadingState.LoadingChunks, 0);

				int t = Options.VideoOptions.RenderDistance;
				//double radiusSquared = Math.Pow(t, 2);

				var target = t * 3d;
				bool allowSpawn = false;
				
                int loaded = 0;
				SpinWait.SpinUntil(() =>
				{
					var playerChunkCoords = new ChunkCoordinates(World.Player.KnownPosition);
					if (_chunksReceived < target)
					{
						progressReport(LoadingState.LoadingChunks, (int)Math.Floor((_chunksReceived / target) * 100d));
                    }
					else if (loaded < _chunksReceived)
					{
						if (_generatingHelper.TryTake(out ChunkColumn chunkColumn, 50))
						{
							if (!allowSpawn)
							{
								if (playerChunkCoords.X == chunkColumn.X && playerChunkCoords.Z == chunkColumn.Z)
								{
									allowSpawn = true;
								}
							}
							
							//base.LoadChunk(chunkColumn, chunkColumn.X, chunkColumn.Z, true);
							/*EventDispatcher.DispatchEvent(new ChunkReceivedEvent(new ChunkCoordinates(chunkColumn.X ,chunkColumn.Z), chunkColumn)
							{
								DoUpdates = true
							})*/
							World.ChunkManager.AddChunk(chunkColumn, new ChunkCoordinates(chunkColumn.X ,chunkColumn.Z), true);
							
							loaded++;
						}

						progressReport(LoadingState.GeneratingVertices, (loaded / _chunksReceived) * 100);
                    }
                    else
					{
						progressReport(LoadingState.Spawning, 99);
                    }

                    return (loaded >= target && allowSpawn && _generatingHelper.Count == 0) || _disconnected; // Spawned || _disconnected;

				});

				hasDoneInitialChunks = true;
			});
		}
		
		private Queue<Entity> _entitySpawnQueue = new Queue<Entity>();

		public void SpawnMob(int entityId, Guid uuid, EntityType type, PlayerLocation position, Vector3 velocity)
		{
			Entity entity = null;
			if (EntityFactory.ModelByNetworkId((long) type, out var renderer, out EntityData knownData))
			{
				if (Enum.TryParse(knownData.Name, out type))
				{
					entity = type.Create(null);
				}

				if (entity == null)
				{
					entity = new Entity((int) type, null, NetworkProvider);
				}

				//if (knownData.Height)
				{
					entity.Height = knownData.Height;
				}

				//if (knownData.Width.HasValue)
					entity.Width = knownData.Width;

				if (string.IsNullOrWhiteSpace(entity.NameTag) && !string.IsNullOrWhiteSpace(knownData.Name))
				{
					entity.NameTag = knownData.Name;
				}
            }

			if (entity == null)
			{
				Log.Warn($"Could not create entity of type: {(int) type}:{type.ToString()}");
				return;
			}

			if (renderer == null)
			{
				var def = Alex.Resources.BedrockResourcePack.EntityDefinitions.FirstOrDefault(x =>
					x.Value.Filename.Replace("_", "").Equals(type.ToString().ToLowerInvariant()));
				if (!string.IsNullOrWhiteSpace(def.Key))
				{
					EntityModel model;
					if (ModelFactory.TryGetModel(def.Value.Geometry["default"],
						    out model) && model != null)
					{
						var textures = def.Value.Textures;
						string texture;
						if (!textures.TryGetValue("default", out texture))
						{
							texture = textures.FirstOrDefault().Value;
						}

						if (Alex.Resources.BedrockResourcePack.Textures.TryGetValue(texture,
							out var bmp))
						{
							PooledTexture2D t = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, bmp);

							renderer = new EntityModelRenderer(model, t);
						}
					}
				}
			}

			if (renderer == null)
			{
				Log.Debug($"Missing renderer for entity: {type.ToString()} ({(int) type})");
				return;
			}

			if (renderer.Texture == null)
			{
				Log.Debug($"Missing texture for entity: {type.ToString()} ({(int) type})");
				return;
			}

			entity.ModelRenderer = renderer;

			entity.KnownPosition = position;
			entity.Velocity = velocity;
			entity.EntityId = entityId;
			entity.UUID = new UUID(uuid.ToByteArray());

			if (!_initiated)
			{
				_entitySpawnQueue.Enqueue(entity);
			}
			else
			{
				base.SpawnEntity(entityId, entity);
			}
		}

		private PlayerLocation _lastReceivedLocation = new PlayerLocation();
		public void UpdatePlayerPosition(PlayerLocation location)
		{
			_lastReceivedLocation = location;

			if (_spawn == Vector3.Zero)
			{
				_spawn = location;
			}
			
			World?.UpdatePlayerPosition(location);
		}

		public void UpdateEntityPosition(long entityId, PlayerLocation location, bool relative)
		{
			World?.UpdateEntityPosition(entityId, location,  relative);
		}

		public void UpdateTime(long worldAge, long timeOfDay)
		{ 
			World?.SetTime(timeOfDay);
		}

		private void SendPacket(Packet packet)
		{
			Client.SendPacket(packet);
		}

		void IJavaProvider.HandlePlay(Packet packet)
		{
			if (packet is KeepAlivePacket keepAlive)
			{
				HandleKeepAlivePacket(keepAlive);
			}
			else if (packet is PlayerPositionAndLookPacket playerPos)
			{
				HandlePlayerPositionAndLookPacket(playerPos);
			}
			else if (packet is ChunkDataPacket chunk)
			{
				HandleChunkData(chunk);
			}
			else if (packet is UpdateLightPacket updateLight)
			{
				HandleUpdateLightPacket(updateLight);
			}
			else if (packet is JoinGamePacket joinGame)
			{
				HandleJoinGamePacket(joinGame);
			}
			else if (packet is UnloadChunk unloadChunk)
			{
				HandleUnloadChunk(unloadChunk);
			}
			else if (packet is ChatMessagePacket chatMessage)
			{
				HandleChatMessagePacket(chatMessage);
			}
			else if (packet is TimeUpdatePacket timeUpdate)
			{
				HandleTimeUpdatePacket(timeUpdate);
			}
			else if (packet is PlayerAbilitiesPacket abilitiesPacket)
			{
				HandlePlayerAbilitiesPacket(abilitiesPacket);
			}
			else if (packet is EntityPropertiesPacket entityProperties)
			{
				HandleEntityPropertiesPacket(entityProperties);
			}
			else if (packet is EntityTeleport teleport)
			{
				HandleEntityTeleport(teleport);
			}
			else if (packet is SpawnMob spawnMob)
			{
				HandleSpawnMob(spawnMob);
			}
			else if (packet is EntityLook look)
			{
				HandleEntityLook(look);
			}
			else if (packet is EntityRelativeMove relative)
			{
				HandleEntityRelativeMove(relative);
			}
			else if (packet is EntityLookAndRelativeMove relativeLookAndMove)
			{
				HandleEntityLookAndRelativeMove(relativeLookAndMove);
			}
			else if (packet is PlayerListItemPacket playerList)
			{
				HandlePlayerListItemPacket(playerList);
			}
			else if (packet is SpawnPlayerPacket spawnPlayerPacket)
			{
				HandleSpawnPlayerPacket(spawnPlayerPacket);
			}
			else if (packet is DestroyEntitiesPacket destroy)
			{
				HandleDestroyEntitiesPacket(destroy);
			}
			else if (packet is EntityHeadLook headlook)
			{
				HandleEntityHeadLook(headlook);
			}
			else if (packet is EntityVelocity velocity)
			{
				HandleEntityVelocity(velocity);
			}
			else if (packet is WindowItems itemsPacket)
			{
				HandleWindowItems(itemsPacket);
			}
			else if (packet is SetSlot setSlotPacket)
			{
				HandleSetSlot(setSlotPacket);
			}
			else if (packet is HeldItemChangePacket pack)
			{
				HandleHeldItemChangePacket(pack);
			}
			else if (packet is EntityStatusPacket entityStatusPacket)
			{
				HandleEntityStatusPacket(entityStatusPacket);
			}
			else if (packet is BlockChangePacket blockChangePacket)
			{
				HandleBlockChangePacket(blockChangePacket);
			}
			else if (packet is MultiBlockChange multiBlock)
			{
				HandleMultiBlockChange(multiBlock);
			}
			else if (packet is TabCompleteClientBound tabComplete)
			{
				HandleTabCompleteClientBound(tabComplete);
			}
			else if (packet is ChangeGameStatePacket p)
			{
				HandleChangeGameStatePacket(p);
			}
			else if (packet is EntityMetadataPacket entityMetadata)
			{
				HandleEntityMetadataPacket(entityMetadata);
			}
			else if (packet is CombatEventPacket combatEventPacket)
			{
				HandleCombatEventPacket(combatEventPacket);
			}
			else if (packet is EntityEquipmentPacket entityEquipmentPacket)
			{
				HandleEntityEquipmentPacket(entityEquipmentPacket);
			}
			else if (packet is RespawnPacket respawnPacket)
			{
				HandleRespawnPacket(respawnPacket);
			}
			else if (packet is TitlePacket titlePacket)
			{
				HandleTitlePacket(titlePacket);
			}
			else if (packet is UpdateHealthPacket healthPacket)
			{
				HandleUpdateHealthPacket(healthPacket);
			}
			else if (packet is DisconnectPacket disconnectPacket)
			{
				HandleDisconnectPacket(disconnectPacket);
			}
			else
			{
				if (UnhandledPackets.TryAdd(packet.PacketId, packet.GetType()))
				{
					Log.Warn($"Unhandled packet: 0x{packet.PacketId:x2} - {packet.ToString()}");
				}
			}
		}

		private void HandleUpdateHealthPacket(UpdateHealthPacket packet)
		{
			World.Player.HealthManager.Health = packet.Health;
			World.Player.HealthManager.Hunger = packet.Food;
			World.Player.HealthManager.Saturation = packet.Saturation;
		}

		private Dictionary<int, Type> UnhandledPackets = new Dictionary<int, Type>();

		private void HandleTitlePacket(TitlePacket packet)
		{
			switch (packet.Action)
			{
				case TitlePacket.ActionEnum.SetTitle:
					TitleComponent.SetTitle(packet.TitleText);
					break;
				case TitlePacket.ActionEnum.SetSubTitle:
					TitleComponent.SetSubtitle(packet.SubtitleText);
                    break;
				case TitlePacket.ActionEnum.SetActionBar:
					
					break;
				case TitlePacket.ActionEnum.SetTimesAndDisplay:
					TitleComponent.SetTimes(packet.FadeIn, packet.Stay, packet.FadeOut);
					break;
				case TitlePacket.ActionEnum.Hide:
					TitleComponent.Hide();
					break;
				case TitlePacket.ActionEnum.Reset:
					TitleComponent.Reset();
					break;
				default:
					Log.Warn($"Unknown Title Action: {(int) packet.Action}");
					break;
			}
		}

		public bool Respawning = false;

		private void HandleRespawnPacket(RespawnPacket packet)
		{

			Respawning = true;
			_dimension = packet.Dimension;
			World.Player.UpdateGamemode(packet.Gamemode);
			//player.


			new Thread(() =>
			{
				LoadingWorldState state = new LoadingWorldState();
				state.UpdateProgress(LoadingState.LoadingChunks, 0);
				Alex.GameStateManager.SetActiveState(state, true);
				World.ChunkManager.ClearChunks();

				int t = Options.VideoOptions.RenderDistance;
				double radiusSquared = Math.Pow(t, 2);

				var target = radiusSquared * 3;

				while (Respawning)
				{
					int chunkProgress = (int) Math.Floor((World.ChunkManager.ChunkCount / target) * 100);
					if (chunkProgress < 100)
					{
						state.UpdateProgress(LoadingState.LoadingChunks, chunkProgress);
					}
					else
					{
						state.UpdateProgress(LoadingState.Spawning, 99);
					}
				}

				Alex.GameStateManager.Back();
			}).Start();
		}

		private Item GetItemFromSlotData(SlotData data)
		{
			if (data == null)
				return new ItemAir();
			
			if (ItemFactory.ResolveItemName(data.ItemID, out string name))
			{
				if (ItemFactory.TryGetItem(name, out Item item))
				{
					item.Count = data.Count;
					item.Nbt = data.Nbt;

					return item;
				}
			}

			return null;
		}

		private void HandleEntityEquipmentPacket(EntityEquipmentPacket packet)
		{
			/*if (packet.Item == null)
			{
				Log.Warn($"Got null item in EntityEquipment.");
				return;
			}*/

			if (World.TryGetEntity(packet.EntityId, out Entity e))
			{
				if (e is Entity entity)
				{
					Item item = GetItemFromSlotData(packet.Item);;

					switch (packet.Slot)
					{
						case EntityEquipmentPacket.SlotEnum.MainHand:
							entity.Inventory.MainHand = item;
							break;
						case EntityEquipmentPacket.SlotEnum.OffHand:
							entity.Inventory.OffHand = item;
							break;
						case EntityEquipmentPacket.SlotEnum.Boots:
							entity.Inventory.Boots = item;
							break;
						case EntityEquipmentPacket.SlotEnum.Leggings:
							entity.Inventory.Leggings = item;
							break;
						case EntityEquipmentPacket.SlotEnum.Chestplate:
							entity.Inventory.Chestplate = item;
							break;
						case EntityEquipmentPacket.SlotEnum.Helmet:
							entity.Inventory.Helmet = item;
							break;
					}
				}
			}
		}

		private void HandleEntityMetadataPacket(EntityMetadataPacket packet)
		{
			//TODO: Handle entity metadata
		}

		private void HandleEntityStatusPacket(EntityStatusPacket packet)
		{
			//TODO: Do somethign with the packet.
		}

		private void HandleCombatEventPacket(CombatEventPacket packet)
		{
			if (packet.Event == CombatEventPacket.CombatEvent.EntityDead)
			{
				Log.Warn($"Status packet: Entity={packet.EntityId} Player={packet.PlayerId} Message={packet.Message}");
				ClientStatusPacket statusPacket = new ClientStatusPacket();
				statusPacket.ActionID = ClientStatusPacket.Action.PerformRespawnOrConfirmLogin;
				SendPacket(statusPacket);
			}
		}

		private void HandleChangeGameStatePacket(ChangeGameStatePacket packet)
		{
			switch (packet.Reason)
			{
				case GameStateReason.InvalidBed:
					break;
				case GameStateReason.EndRain:
					World?.SetRain(false);
					break;
				case GameStateReason.StartRain:
					World?.SetRain(true);
					break;
				case GameStateReason.ChangeGamemode:
					if (World?.Player is Player player)
					{
						player.UpdateGamemode((Gamemode) packet.Value);
					}
					break;
				case GameStateReason.ExitEnd:
					break;
				case GameStateReason.DemoMessage:
					break;
				case GameStateReason.ArrowHitPlayer:
					break;
				case GameStateReason.FadeValue:
					break;
				case GameStateReason.FadeTime:
					break;
				case GameStateReason.PlayerElderGuardianMob:
					break;
			}
		}

		private void HandleTabCompleteClientBound(TabCompleteClientBound tabComplete)
		{
			//TODO: Re-implement tab complete
			Log.Info($"!!! TODO: Re-implement tab complete.");
			//ChatReceiver?.ReceivedTabComplete(tabComplete.TransactionId, tabComplete.Start, tabComplete.Length, tabComplete.Matches);
		}

		private void HandleMultiBlockChange(MultiBlockChange packet)
		{
			//throw new NotImplementedException();
			int cx = packet.ChunkX * 16;
			int cz = packet.ChunkZ * 16;
			foreach (var blockUpdate in packet.Records)
			{
				//WorldReceiver?.SetBlockState(new BlockCoordinates(blockUpdate.RelativeX + cx, blockUpdate.Y, blockUpdate.RelativeZ + cz), BlockFactory.GetBlockState(blockUpdate.BlockId));
			}
		}

		private void HandleBlockChangePacket(BlockChangePacket packet)
		{
			//throw new NotImplementedException();
			World?.SetBlockState(packet.Location, BlockFactory.GetBlockState((uint) packet.PalleteId));
		}

		private void HandleHeldItemChangePacket(HeldItemChangePacket packet)
		{
			if (World?.Player is Player player)
			{
				player.Inventory.SelectedSlot = packet.Slot;
			}
		}

		private void HandleSetSlot(SetSlot packet)
		{
			Inventory inventory = null;
			if (packet.WindowId == 0 || packet.WindowId == -2)
			{
				if (World?.Player is Player player)
				{
					inventory = player.Inventory;
				}
			}

			if (inventory == null) return;

			if (packet.SlotId < inventory.SlotCount)
			{
				inventory[packet.SlotId] = GetItemFromSlotData(packet.Slot);
			}
		}

		private void HandleWindowItems(WindowItems packet)
		{
			Inventory inventory = null;
			if (packet.WindowId == 0)
			{
				if (World?.Player is Player player)
				{
					inventory = player.Inventory;
				}
			}

			if (inventory == null) return;

			if (packet.Slots != null && packet.Slots.Length > 0)
			{
				for (int i = 0; i < packet.Slots.Length; i++)
				{
					if (i >= inventory.SlotCount)
					{
						Log.Warn($"Slot index {i} is out of bounds (Max: {inventory.SlotCount})");
						continue;
					}
					inventory[i] = GetItemFromSlotData(packet.Slots[i]);
				}
			}
		}

		private void HandleDestroyEntitiesPacket(DestroyEntitiesPacket packet)
		{
			foreach(var id in packet.Entitys)
			{
				var p = _players.ToArray().FirstOrDefault(x => x.Value.EntityId == id);
				if (p.Key != null)
				{
					_players.TryRemove(p.Key, out _);
				}

				base.DespawnEntity(id);
			}
		}

		private void HandleSpawnPlayerPacket(SpawnPlayerPacket packet)
		{
			if (_players.TryGetValue(new UUID(packet.Uuid.ToByteArray()), out PlayerMob mob))
			{
				float yaw = MathUtils.AngleToNotchianDegree(packet.Yaw);
				mob.KnownPosition = new PlayerLocation(packet.X, packet.Y, packet.Z, yaw, yaw, MathUtils.AngleToNotchianDegree(packet.Pitch));
				mob.EntityId = packet.EntityId;
				mob.IsSpawned = true;

				base.SpawnEntity(packet.EntityId, mob);
			}
		}

		private ConcurrentDictionary<UUID, PlayerMob> _players = new ConcurrentDictionary<UUID, PlayerMob>();
		private void HandlePlayerListItemPacket(PlayerListItemPacket packet)
		{
			Alex.Resources.ResourcePack.TryGetBitmap("entity/alex", out var rawTexture);
	        var t = TextureUtils.BitmapToTexture2D(Alex.GraphicsDevice, rawTexture);

			if (packet.Action == PlayerListAction.AddPlayer)
			{
				//ThreadPool.QueueUserWorkItem(state =>
				//{
					foreach (var entry in packet.AddPlayerEntries)
					{
						string skinJson = null;
						bool skinSlim = true;
						foreach (var property in entry.Properties)
						{
							if (property.Name == "textures")
							{
								skinJson = Encoding.UTF8.GetString(Convert.FromBase64String(property.Value));
								
							}
						}

						PlayerMob entity = new PlayerMob(entry.Name, (World) World, NetworkProvider, t, skinSlim ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom");
						entity.UpdateGamemode((Gamemode) entry.Gamemode);
						entity.UUID = new UUID(entry.UUID.ToByteArray());

						World?.AddPlayerListItem(new PlayerListItem(entity.Uuid, entry.Name,
							(Gamemode) entry.Gamemode, entry.Ping));

						if (entry.HasDisplayName)
						{
							if (ChatObject.TryParse(entry.DisplayName, out ChatObject chat))
							{
								entity.NameTag = chat.RawMessage;
							}
							else
							{
								entity.NameTag = entry.DisplayName;
							}
						}
						else
						{
							entity.NameTag = entry.Name;
						}

						entity.HideNameTag = false;
						entity.IsAlwaysShowName = true;

						if (_players.TryAdd(entity.UUID, entity) && skinJson != null)
						{
							Alex.UIThreadQueue.Enqueue(() =>
							{
								if (SkinUtils.TryGetSkin(skinJson, Alex.GraphicsDevice, out var skin, out skinSlim))
								{
									t = skin;
									
									entity.GeometryName =
										skinSlim ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom";
								
									entity.UpdateSkin(t);
									
								//	Log.Info($"Skin update!");
								}
							});
						}
					}
				//});
			}

			else if (packet.Action == PlayerListAction.UpdateDisplayName)
			{
				foreach (var entry in packet.UpdateDisplayNameEntries)
				{
					if (_players.TryGetValue(new UUID(entry.UUID.ToByteArray()), out PlayerMob entity))
					{
						if (entry.HasDisplayName)
						{
							if (ChatObject.TryParse(entry.DisplayName, out ChatObject chat))
							{
								entity.NameTag = chat.RawMessage;
							}
							else
							{
								entity.NameTag = entry.DisplayName;
							}
						}
						else
						{
							entity.NameTag = entity.Name;
						}
					}
				}
			}

			else if (packet.Action == PlayerListAction.RemovePlayer)
			{
				foreach (var remove in packet.RemovePlayerEntries)
				{
					var uuid = new UUID(remove.UUID.ToByteArray());
					World?.RemovePlayerListItem(uuid);
				//	API.Utils.UUID uuid = new UUID(remove.UUID.ToByteArray());
				/*	if (_players.TryRemove(uuid, out PlayerMob removed))
					{
						if (removed.IsSpawned)
						{
							base.DespawnEntity(removed.EntityId);
						}
					}*/
				}
			}
		}

		private void HandleEntityLookAndRelativeMove(EntityLookAndRelativeMove packet)
		{
			var yaw = MathUtils.AngleToNotchianDegree(packet.Yaw);
			World.UpdateEntityPosition(packet.EntityId, new PlayerLocation(MathUtils.FromFixedPoint(packet.DeltaX),
				MathUtils.FromFixedPoint(packet.DeltaY),
				MathUtils.FromFixedPoint(packet.DeltaZ),
				yaw, 
				yaw,
				MathUtils.AngleToNotchianDegree(packet.Pitch))
			{
				OnGround = packet.OnGround
			}, true, true, true);
		}

		private void HandleEntityRelativeMove(EntityRelativeMove packet)
		{
			World.UpdateEntityPosition(packet.EntityId, new PlayerLocation(MathUtils.FromFixedPoint(packet.DeltaX), MathUtils.FromFixedPoint(packet.DeltaY), MathUtils.FromFixedPoint(packet.DeltaZ))
			{
				OnGround = packet.OnGround
			}, true);
		}

		private void HandleEntityHeadLook(EntityHeadLook packet)
		{
			if (World.TryGetEntity(packet.EntityId, out var entity))
			{
				entity.KnownPosition.HeadYaw = MathUtils.AngleToNotchianDegree(packet.HeadYaw);
				//entity.UpdateHeadYaw(MathUtils.AngleToNotchianDegree(packet.HeadYaw));
			}
		}

		private void HandleEntityLook(EntityLook packet)
		{
			World.UpdateEntityLook(packet.EntityId, MathUtils.AngleToNotchianDegree(packet.Yaw), MathUtils.AngleToNotchianDegree(packet.Pitch), packet.OnGround);
		}

		private void HandleEntityTeleport(EntityTeleport packet)
		{
			float yaw = MathUtils.AngleToNotchianDegree(packet.Yaw);
			World.UpdateEntityPosition(packet.EntityID, new PlayerLocation(packet.X, packet.Y, packet.Z, yaw, yaw, MathUtils.AngleToNotchianDegree(packet.Pitch))
			{
				OnGround = packet.OnGround
			}, updateLook: true, updatePitch:true);
		}

		private void HandleEntityVelocity(EntityVelocity packet)
		{
			Entity entity;
			if (!World.TryGetEntity(packet.EntityId, out entity))
			{
				if (packet.EntityId == World.Player.EntityId)
				{
					entity = World.Player;
				}
			}

			if (entity != null)
			{
				var velocity = new Vector3(
					packet.VelocityX / 8000f, packet.VelocityY / 8000f, packet.VelocityZ / 8000f) * 20f;

				var old = entity.Velocity;

				entity.Velocity += new Microsoft.Xna.Framework.Vector3(
					velocity.X - old.X, velocity.Y - old.Y, velocity.Z - old.Z);
			}
		}

		private void HandleEntityPropertiesPacket(EntityPropertiesPacket packet)
		{
			if (packet.EntityId == 0 || packet.EntityId == _entityId)
			{
				if (World.Player is Player player)
				{
					foreach (var prop in packet.Properties.Values)
					{
						if (prop.Key.Equals("generic.movementSpeed", StringComparison.InvariantCultureIgnoreCase))
						{
						//	player.MovementSpeed = prop.Value;
						}
						else if (prop.Key.Equals("generic.flyingSpeed", StringComparison.InvariantCultureIgnoreCase))
						{
						//	player.FlyingSpeed = prop.Value;
						}

						//TODO: Modifier data
					}
				}
			}
		}

		private void HandlePlayerAbilitiesPacket(PlayerAbilitiesPacket packet)
		{
			var flags = packet.Flags;
			if (World?.Player is Player player)
			{
				player.CanFly = flags.IsBitSet(0x03);
				player.Invulnerable = flags.IsBitSet(0x00);

				if (flags.IsBitSet(0x01))
				{
					player.IsFlying = true;
					_flying = true;
				}
				else
				{
					player.IsFlying = false;
					_flying = false;
				}
			}
		}

		private void HandleTimeUpdatePacket(TimeUpdatePacket packet)
		{
			UpdateTime(packet.WorldAge, packet.TimeOfDay);
		}

		private void HandleChatMessagePacket(ChatMessagePacket packet)
		{
			/*try
			{
				var decodedChatComponent =
					JsonConvert.DeserializeObject<BaseComponent>(packet.Message, new BaseComponentSerializer());
			}
			catch (Exception ex)
			{
				Log.Warn($"Failed to decode: " + ex);
			}*/

			if (ChatObject.TryParse(packet.Message, out ChatObject chat))
			{
				EventDispatcher.DispatchEvent(new ChatMessageReceivedEvent(chat));
			}
			else
			{
				Log.Warn($"Failed to parse chat object, received json: {packet.Message}");
			}
		}

		private void HandleUnloadChunk(UnloadChunk packet)
		{
			EventDispatcher.DispatchEvent(new ChunkUnloadEvent(new ChunkCoordinates(packet.X, packet.Z)));
		}

		private int _entityId = -1;
		private int _dimension = 0;
		private void HandleJoinGamePacket(JoinGamePacket packet)
		{
			_dimension = packet.Dimension;

			ClientSettingsPacket settings = new ClientSettingsPacket();
			settings.ChatColors = true;
			settings.ChatMode = 0;
			settings.ViewDistance = (byte)Options.VideoOptions.RenderDistance;
			settings.SkinParts = 255;
			settings.MainHand = 1;
			settings.Locale = "en_US";
			SendPacket(settings);

			_entityId = packet.EntityId;
			if (World?.Player is Player player)
			{
				player.EntityId = packet.EntityId;
				player.UpdateGamemode((Gamemode)packet.Gamemode);
			}
			else
			{
				Log.Warn($"Could not get player entity!");
			}
		}

		private void HandleUpdateLightPacket(UpdateLightPacket packet)
		{
			if (World.GetChunkColumn(packet.ChunkX, packet.ChunkZ) is ChunkColumn c)
			{
				for (int i = 1; i < packet.SkyLightArrays.Length - 1; i++)
				{
					byte[] data = packet.SkyLightArrays[i];
					if (data == null) continue;

					NibbleArray n = new NibbleArray();
					n.Data = data;

					c.Sections[i].SkyLight = n;
				}

				for (int i = 1; i < packet.BlockLightArrays.Length - 1; i++)
				{
					byte[] data = packet.BlockLightArrays[i];
					if (data == null) continue;

					NibbleArray n = new NibbleArray();
					n.Data = data;

					c.Sections[i].BlockLight = n;
				}

				World.ChunkManager.ScheduleChunkUpdate(new ChunkCoordinates(packet.ChunkX, packet.ChunkZ), ScheduleType.Full, false);//.ChunkUpdate(c, ScheduleType.Full);
            }
        }

        //private BlockingCollection<ChunkDataPacket> _chunkQueue = new BlockingCollection<ChunkDataPacket>();
        private void HandleChunkData(ChunkDataPacket chunk)
		{
			_loginCompleteEvent?.Set();
			//_chunkQueue.Add(chunk);
			ThreadPool.QueueUserWorkItem(() =>
			{
				ChunkColumn result = null;// = new ChunkColumn();
				if (chunk.GroundUp)
				{
					result = new ChunkColumn();
				}
				else
				{
					if (World.GetChunkColumn(chunk.ChunkX, chunk.ChunkZ) is ChunkColumn c)
					{
						result = c;
					}
					else
					{
						result = new ChunkColumn();
					}
				}

				result.X = chunk.ChunkX;
				result.Z = chunk.ChunkZ;
				result.IsDirty = true;
				
				result.Read(new MinecraftStream(new MemoryStream(chunk.Buffer)), chunk.PrimaryBitmask, chunk.GroundUp, _dimension == 0);
				result.SkyLightDirty = true;
				result.BlockLightDirty = true;
				
				if (!hasDoneInitialChunks)
				{
					_generatingHelper.Add(result);
					_chunksReceived++;
					return;
				}

				World.ChunkManager.AddChunk(result, new ChunkCoordinates(result.X ,result.Z), true);
			});
		}

		private void HandleKeepAlivePacket(KeepAlivePacket packet)
		{
			KeepAlivePacket response = new KeepAlivePacket();
			response.KeepAliveid = packet.KeepAliveid;
			//response.PacketId = 0x0E;

			SendPacket(response);
		}

		private void HandlePlayerPositionAndLookPacket(PlayerPositionAndLookPacket packet)
		{
			Respawning = false;

			//_spawned = true;
			TeleportConfirm confirmation = new TeleportConfirm();
			confirmation.TeleportId = packet.TeleportId;
			SendPacket(confirmation);

			PlayerPositionAndLookPacketServerBound response = new PlayerPositionAndLookPacketServerBound();
			response.OnGround = false;
			response.Pitch = packet.Pitch;
			response.Yaw = packet.Yaw;
			response.X = packet.X;
			response.Y = packet.Y;
			response.Z = packet.Z;

			SendPacket(response);

			UpdatePlayerPosition(
				new PlayerLocation(packet.X, packet.Y, packet.Z, packet.Yaw, packet.Yaw, pitch: packet.Pitch));

			if (!Spawned)
			{
				ClientStatusPacket clientStatus = new ClientStatusPacket();
				clientStatus.ActionID = ClientStatusPacket.Action.PerformRespawnOrConfirmLogin;
				SendPacket(clientStatus);
				
				Spawned = true;
			}
		}

		void IJavaProvider.HandleHandshake(Packet packet)
		{

		}

		void IJavaProvider.HandleStatus(Packet packet)
		{

		}

		void IJavaProvider.HandleLogin(Packet packet)
		{
			if (packet is DisconnectPacket disconnect)
			{
				HandleDisconnectPacket(disconnect);
			}
			else if (packet is EncryptionRequestPacket)
			{
				HandleEncryptionRequest((EncryptionRequestPacket)packet);
			}
			else if (packet is SetCompressionPacket compression)
			{
				HandleSetCompression(compression);
			}
			else if (packet is LoginSuccessPacket success)
			{
				HandleLoginSuccess(success);
			}
		}

		private void HandleSpawnMob(SpawnMob packet)
		{
			SpawnMob(packet.EntityId, packet.Uuid, (EntityType)packet.Type, new PlayerLocation(packet.X, packet.Y, packet.Z, packet.Yaw, packet.Yaw, packet.Pitch)
			{
			//	OnGround = packet.
			}, new Vector3(packet.VelocityX, packet.VelocityY, packet.VelocityZ));
		}

		private void HandleDisconnectPacket(DisconnectPacket packet)
		{
			if (ChatObject.TryParse(packet.Message, out ChatObject o))
			{
				ShowDisconnect(o.RawMessage);
			}
			else
			{
				ShowDisconnect(packet.Message);
			}

			_disconnected = true;
			Log.Info($"Received disconnect: {packet.Message}");
			Client.Stop();
		}

		public bool Spawned = false;
		private void HandleLoginSuccess(LoginSuccessPacket packet)
		{
			Client.ConnectionState = ConnectionState.Play;
			
			//Client.UsePacketHandlerQueue = true;
		}

		private void HandleSetCompression(SetCompressionPacket packet)
		{
			Client.CompressionThreshold = packet.Threshold;
			Client.CompressionEnabled = true;
		}

		private string _accesToken = "";
		private string _uuid = "";
		private string _username = "";
		private byte[] SharedSecret = new byte[16];
		private void HandleEncryptionRequest(EncryptionRequestPacket packet)
		{
			Random random = new Random();
			random.NextBytes(SharedSecret);

			string serverHash;
			using (MemoryStream ms = new MemoryStream())
			{
				byte[] ascii = Encoding.ASCII.GetBytes(packet.ServerId);
				ms.Write(ascii, 0, ascii.Length);
				ms.Write(SharedSecret, 0, 16);
				ms.Write(packet.PublicKey, 0, packet.PublicKey.Length);

				serverHash = JavaHexDigest(ms.ToArray());
			}

			bool authenticated = true;
			if (!string.IsNullOrWhiteSpace(_accesToken))
			{
				try
				{	
					var baseAddress = "https://sessionserver.mojang.com/session/minecraft/join";

					var http = (HttpWebRequest) WebRequest.Create(new Uri(baseAddress));
					http.Accept = "application/json";
					http.ContentType = "application/json";
					http.Method = "POST";

					var bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new JoinRequest()
					{
						ServerId = serverHash,
						SelectedProfile = _uuid,
						AccessToken = _accesToken
					}));

					using (Stream newStream = http.GetRequestStream())
					{
						newStream.Write(bytes, 0, bytes.Length);
					}

					var r = http.GetResponse();

					using (var stream = r.GetResponseStream())
					using (var sr = new StreamReader(stream))
					{
						var content = sr.ReadToEnd();
					}
				}
				catch
				{
					authenticated = false;
				}
			}
			else
			{
				authenticated = false;
			}

			if (!authenticated)
			{
				ShowDisconnect("disconnect.loginFailedInfo.invalidSession", true);
				return;
			}

			var cryptoProvider = AsnKeyBuilder.DecodePublicKey(packet.PublicKey);
			Log.Info($"Crypto: {cryptoProvider == null} Pub: {packet.PublicKey} Shared: {SharedSecret}");
			var encrypted = cryptoProvider.Encrypt(SharedSecret, RSAEncryptionPadding.Pkcs1);

			EncryptionResponsePacket response = new EncryptionResponsePacket();
			response.SharedSecret = encrypted;
			response.VerifyToken = cryptoProvider.Encrypt(packet.VerifyToken, RSAEncryptionPadding.Pkcs1);
			SendPacket(response);

			Client.InitEncryption(SharedSecret);
		}

		private bool Login(string username, string uuid, string accessToken)
		{
			try
			{
				//	_loginCompleteEvent = signalWhenReady;
				_username = username;
				_uuid = uuid;
				_accesToken = accessToken;

				var ar = TcpClient.BeginConnect(Endpoint.Address, Endpoint.Port, null, null);
				using (ar.AsyncWaitHandle)
				{
					if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))
					{
						TcpClient.Close();
						return false;
					}

					TcpClient.EndConnect(ar);
				}

			//TcpClient.Connect(Endpoint);
				//	ServerBound.InitEncryption();
				Client.Initialize();

				HandshakePacket handshake = new HandshakePacket();
				handshake.NextState = ConnectionState.Login;
				handshake.ServerAddress = Hostname;
				handshake.ServerPort = (ushort) Endpoint.Port;
				handshake.ProtocolVersion = JavaProtocol.ProtocolVersion;
				SendPacket(handshake);

				Client.ConnectionState = ConnectionState.Login;

				LoginStartPacket loginStart = new LoginStartPacket();
				loginStart.Username = _username;
				SendPacket(loginStart);
			}
			catch (SocketException)
			{
				return false;
			}
			catch (Exception ex)
			{
				ShowDisconnect(ex.Message);
			}

			return true;
		}

		public sealed class JoinRequest
		{
			[JsonProperty("accessToken")]
			public string AccessToken;

			[JsonProperty("selectedProfile")]
			public string SelectedProfile;

			[JsonProperty("serverId")]
			public string ServerId;
		}

		private static string JavaHexDigest(byte[] input)
		{
			var sha1 = SHA1.Create();
			byte[] hash = sha1.ComputeHash(input);
			bool negative = (hash[0] & 0x80) == 0x80;
			if (negative) // check for negative hashes
				hash = TwosCompliment(hash);
			// Create the string and trim away the zeroes
			string digest = GetHexString(hash).TrimStart('0');
			if (negative)
				digest = "-" + digest;
			return digest;
		}

		private static string GetHexString(byte[] p)
		{
			string result = string.Empty;
			for (int i = 0; i < p.Length; i++)
				result += p[i].ToString("x2"); // Converts to hex string
			return result;
		}

		private static byte[] TwosCompliment(byte[] p) // little endian
		{
			int i;
			bool carry = true;
			for (i = p.Length - 1; i >= 0; i--)
			{
				p[i] = (byte)~p[i];
				if (carry)
				{
					carry = p[i] == 0xFF;
					p[i]++;
				}
			}
			return p;
		}

		public override void Dispose()
		{
			Spawned = false;
			_initiated = false;
			
			base.Dispose();
			_gameTickTimer?.Dispose();

			Client.Stop();
			TcpClient.Dispose();

			Client.Dispose();
		}
	}
}
