using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Data;
using Alex.API.Data.Options;
using Alex.API.Graphics;
using Alex.API.Graphics.GpuResources;
using Alex.API.Input;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.Utils.Collections;
using Alex.API.World;
using Alex.Blocks;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Entities.Effects;
using Alex.Entities.Generic;
using Alex.Entities.Passive;
using Alex.Entities.Projectiles;
using Alex.Gamestates;
using Alex.Graphics.Models.Entity;
using Alex.Gui.Dialogs.Containers;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Net;
using Alex.Networking.Java;
using Alex.Networking.Java.Events;
using Alex.Networking.Java.Packets;
using Alex.Networking.Java.Packets.Handshake;
using Alex.Networking.Java.Packets.Login;
using Alex.Networking.Java.Packets.Play;
using Alex.Networking.Java.Util;
using Alex.Networking.Java.Util.Encryption;
using Alex.Particles;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using Alex.Utils.Inventories;
using Alex.Worlds.Abstraction;
using fNbt;
using Microsoft.Extensions.DependencyInjection;
using MiNET;
using MiNET.Entities;
using MiNET.Plugins;
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using Org.BouncyCastle.Utilities.Encoders;
using RocketUI.Input;
using BlockCoordinates = Alex.API.Utils.Vectors.BlockCoordinates;
using ChunkColumn = Alex.Worlds.Chunks.ChunkColumn;
using ChunkCoordinates = Alex.API.Utils.Vectors.ChunkCoordinates;
using Command = Alex.Utils.Command;
using CommandProperty = Alex.Utils.CommandProperty;
using ConnectionState = Alex.Networking.Java.ConnectionState;
using Entity = Alex.Entities.Entity;
using MessageType = Alex.API.Data.MessageType;
using Mob = Alex.Entities.Mob;
using NibbleArray = Alex.API.Utils.NibbleArray;
using Packet = Alex.Networking.Java.Packets.Packet;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.Vectors.PlayerLocation;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace Alex.Worlds.Multiplayer.Java
{
	public class JavaWorldProvider : WorldProvider, IPacketHandler
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		private Alex Alex { get; }
		private NetConnection Client { get; }
		private PlayerProfile Profile { get; }
		
		private IOptionsProvider OptionsProvider { get; }
		private AlexOptions Options => OptionsProvider.AlexOptions;

		private IPEndPoint Endpoint;
		private ManualResetEvent _loginCompleteEvent = new ManualResetEvent(false);

		//private DedicatedThreadPool ThreadPool;
		public string Hostname { get; set; }
		
		private          JavaNetworkProvider NetworkProvider { get; }
		private JavaCommandProvider CommandProvider { get; }
		private readonly List<IDisposable>   _disposables = new List<IDisposable>();
		public JavaWorldProvider(Alex alex, IPEndPoint endPoint, PlayerProfile profile, out NetworkProvider networkProvider)
		{
			Alex = alex;
			Profile = profile;
			Endpoint = endPoint;

			OptionsProvider = alex.Services.GetRequiredService<IOptionsProvider>();
			//	ThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount));
		
			Client = new NetConnection(endPoint, CancellationToken.None);
			Client.OnConnectionClosed += OnConnectionClosed;
			Client.PacketHandler = this;
			
			NetworkProvider = new JavaNetworkProvider(Client);;
			networkProvider = NetworkProvider;

			_disposables.Add(Options.VideoOptions.RenderDistance.Bind(RenderDistanceSettingChanged));
			CommandProvider = new JavaCommandProvider(this, Client);
			NetworkProvider.CommandProvider = CommandProvider;
		}

		private void RenderDistanceSettingChanged(int oldvalue, int newvalue)
		{
			SendSettings();
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

		private bool _disconnectShown = false;
		public void ShowDisconnect(string reason, bool useTranslation = false, bool force = false)
		{
			if (_disconnectShown && force && Alex.GameStateManager.GetActiveState() is DisconnectedScreen s)
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

		private PlayerLocation _lastSentLocation = new PlayerLocation(Vector3.Zero);
		private int _tickSinceLastPositionUpdate = 0;
		private bool _flying = false;

		public void SendSettings()
		{
			ClientSettingsPacket settings = ClientSettingsPacket.CreateObject();
			settings.ChatColors = true;
			settings.ChatMode = 0;
			settings.ViewDistance = (byte) World.ChunkManager.RenderDistance;
			settings.SkinParts = World.Player.SkinFlags.Value;// 255;
			settings.MainHand = World.Player.IsLeftHanded ? 0 : 1;
			settings.Locale = Alex.GuiRenderer.Language.Code; //Options.MiscelaneousOptions.Language.Value;
			
			SendPacket(settings);
		}
		
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

			PlayerAbilitiesPacket abilitiesPacket = PlayerAbilitiesPacket.CreateObject();
			abilitiesPacket.ServerBound = true;

			abilitiesPacket.Flags = (byte) flags;
			
			//abilitiesPacket.FlyingSpeed = (float) player.FlyingSpeed;
			//abilitiesPacket.WalkingSpeed = (float)player.MovementSpeed;

			SendPacket(abilitiesPacket);
		}

		public override void OnTick()
		{
			if (World == null) return;

		//	var isTick = _isRealTick;
			//_isRealTick = !isTick;

			//if (!_initiated) return;
			
			var player = World.Player;
			if (player != null && player.IsSpawned && Client.ConnectionState == ConnectionState.Play)
			{
				Client.Latency = player.Latency;
				//player.IsSpawned = Spawned;

				//if (isTick)
				{
					if (player.IsFlying != _flying)
					{
						_flying = player.IsFlying;

						SendPlayerAbilities(player);
					}
				}

				var pos = player.KnownPosition;
					
				//Log.Info($"Tick... (Distance: {Vector3.DistanceSquared(pos.ToVector3(), _lastSentLocation.ToVector3())})");
					
				if (Math.Abs(pos.DistanceTo(_lastSentLocation)) > 0.0f)
				{
					SendPlayerPositionAndLook(pos, SendPositionReason.Tick);
					//World.ChunkManager.FlagPrioritization();
				}
				else if (Math.Abs(pos.Pitch - _lastSentLocation.Pitch) > 0f || Math.Abs(pos.HeadYaw - _lastSentLocation.Yaw) > 0f)
				{
					SendPlayerLook(pos, SendPositionReason.Tick);

					//_tickSinceLastPositionUpdate = 0;
						
					//World.ChunkManager.FlagPrioritization();
						
					//_lastSentLocation.Pitch = pos.Pitch;
					//_lastSentLocation.Yaw = pos.HeadYaw;
				}
				else if (_tickSinceLastPositionUpdate >= 20)
				{
					SendPlayerPosition(pos, SendPositionReason.Tick);
				}
				else
				{
					_tickSinceLastPositionUpdate++;
				}
			}
		}

		private float FixPitch(float pitch)
		{
			if (pitch >= 270f && pitch <= 360f)
			{
				return -(360f - pitch);
			}

			return pitch;
		}

		private enum SendPositionReason
		{
			Tick,
			Respawn,
			Server,
			Other
		}

		private void SendPlayerPosition(PlayerLocation pos, SendPositionReason reason = SendPositionReason.Other)
		{
			//Log.Info($"Sending PlayerPosition: {reason}");
			
			PlayerPosition packet = PlayerPosition.CreateObject();
			packet.FeetY = pos.Y;
			packet.X = pos.X;
			packet.Z = pos.Z;
			packet.OnGround = pos.OnGround;

			SendPacket(packet);
			
			_lastSentLocation = pos;
			_tickSinceLastPositionUpdate = 0;
		}

		private void SendPlayerPositionAndLook(PlayerLocation pos, SendPositionReason reason = SendPositionReason.Other)
		{
			//Log.Info($"Sending PlayerPositionAndLook: {reason}");
			
			PlayerPositionAndLookPacketServerBound packet = PlayerPositionAndLookPacketServerBound.CreateObject();
			packet.Yaw = 360f - pos.HeadYaw;
			packet.Pitch = FixPitch(pos.Pitch);
			packet.X = pos.X;
			packet.Y = pos.Y;
			packet.Z = pos.Z;
			packet.OnGround = pos.OnGround;

			SendPacket(packet);
			
			_lastSentLocation = pos;
			_tickSinceLastPositionUpdate = 0;
		}

		private void SendPlayerLook(PlayerLocation pos, SendPositionReason reason = SendPositionReason.Other)
		{
			//Log.Info($"Sending playerlook: {reason}");
			PlayerLookPacket playerLook = PlayerLookPacket.CreateObject();
			playerLook.Pitch = FixPitch(pos.Pitch);
			playerLook.Yaw = 360f - pos.HeadYaw;
			playerLook.OnGround = pos.OnGround;

			SendPacket(playerLook);
		}

		private ManagedTexture2D _alexSkin;
		private Vector3 _spawn = Vector3.Zero;
		public override Vector3 GetSpawnPoint()
		{
			return _spawn;
		}

		protected override void Initiate()
		{
			//	World?.UpdatePlayerPosition(_lastReceivedLocation);

			Alex.Resources.TryGetBitmap("entity/alex", out var rawTexture);
			_alexSkin = TextureUtils.BitmapToTexture2D(this, Alex.GraphicsDevice, rawTexture);
			//_initiated = true;
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

		private bool                            _hasDoneInitialChunks = false;
		private BlockingCollection<ChunkColumn> _generatingHelper     = new BlockingCollection<ChunkColumn>();
		private int                             _chunksReceived       = 0;

		public override LoadResult Load(ProgressReport progressReport)
		{
			if (!Client.Initialize(CancellationToken.None))
				return LoadResult.Failed;
			
			progressReport(LoadingState.ConnectingToServer, 0);

			if (!Login(Profile.PlayerName, Profile.Uuid, Profile.AccessToken))
			{
				_disconnected = true;

				return LoadResult.LoginFailed;
			}

			if (_disconnected) return LoadResult.ConnectionLost;

			progressReport(LoadingState.ConnectingToServer, 99);

			if (!_loginCompleteEvent.WaitOne(5000))
				return LoadResult.Timeout;

			if (_disconnected) return LoadResult.ConnectionLost;

			progressReport(LoadingState.LoadingChunks, 0);

			//double radiusSquared = Math.Pow(t, 2);


			bool allowSpawn = false;

			World.Player.WaitingOnChunk = true;

			int loaded = 0;

			SpinWait.SpinUntil(
				() =>
				{
					int    t             = World.ChunkManager.RenderDistance;
					double radiusSquared = Math.Pow(t, 2);
					var    target        = radiusSquared;

					var playerChunkCoords = new ChunkCoordinates(World.Player.KnownPosition);

					if (_chunksReceived >= target && !_generatingHelper.IsAddingCompleted)
					{
						_generatingHelper.CompleteAdding();
					}

					if (_chunksReceived < target)
					{
						//	Log.Info($"Chunks: {_chunksReceived} / {target}");
						progressReport(LoadingState.LoadingChunks, (int) Math.Floor((100 / target) * _chunksReceived));
					}
					else if (loaded < target || !allowSpawn || _generatingHelper.Count > 0)
					{
						if (_generatingHelper.TryTake(out ChunkColumn chunkColumn, 50))
						{
							//base.LoadChunk(chunkColumn, chunkColumn.X, chunkColumn.Z, true);
							/*EventDispatcher.DispatchEvent(new ChunkReceivedEvent(new ChunkCoordinates(chunkColumn.X ,chunkColumn.Z), chunkColumn)
							{
								DoUpdates = true
							})*/
							World.ChunkManager.AddChunk(
								chunkColumn, new ChunkCoordinates(chunkColumn.X, chunkColumn.Z), true);

							loaded++;
						}

						if (!allowSpawn)
						{
							if (World.ChunkManager.TryGetChunk(playerChunkCoords, out _))
							{
								allowSpawn = true;
							}
						}

						if (!allowSpawn && !World.Player.WaitingOnChunk)
						{
							allowSpawn = true;
						}

						if (loaded >= target)
						{
							int p = allowSpawn ? 50 : 0;

							if (ReadyToSpawn)
							{
								p += 25;
							}

							progressReport(LoadingState.Spawning, p);
						}
						else
						{
							progressReport(LoadingState.GeneratingVertices, (int) Math.Floor((100 / target) * loaded));
						}
					}
					else
					{
						_hasDoneInitialChunks = true;
						progressReport(LoadingState.Spawning, 99);
					}

					return (loaded >= target && allowSpawn && _hasDoneInitialChunks && ReadyToSpawn)
					       || _disconnected; // Spawned || _disconnected;
				});

			World.Player.OnSpawn();

			World.Player.Inventory.CursorChanged += InventoryOnCursorChanged;
			World.Player.Inventory.Closed += (sender, args) => { ClosedContainer(0); };

			return LoadResult.Done;
		}
		

		public Entity SpawnMob(int entityId, MiNET.Utils.UUID uuid, EntityType type, PlayerLocation position, Vector3 velocity)
		{
			if ((int) type == 37) //Item
			{
				ItemEntity itemEntity = new ItemEntity(null);
				itemEntity.EntityId = entityId;
				itemEntity.Velocity = velocity;
				itemEntity.KnownPosition = position;
				
				//itemEntity.SetItem(itemClone);

				if (World.SpawnEntity(itemEntity))
				{
					return itemEntity;
				}
				else
				{
					Log.Warn($"Could not spawn in item entity, an entity with this entity id already exists! (Runtime: {entityId})");
				}
				
				return null;
			}
			else if ((int) type == 26)
			{
				EntityFallingBlock itemEntity = new EntityFallingBlock(null);
				itemEntity.EntityId = entityId;
				itemEntity.Velocity = velocity;
				itemEntity.KnownPosition = position;
				
				//itemEntity.SetItem(itemClone);

				if (World.SpawnEntity(itemEntity))
				{
					return itemEntity;
				}
				else
				{
					Log.Warn($"Could not spawn in item entity, an entity with this entity id already exists! (Runtime: {entityId})");
				}
				
				return null;
			}
			
			Entity entity = null;
			EntityModel model;
			if (EntityFactory.ModelByNetworkId((long) type, out var renderer, out EntityData knownData))
			{
				type = MiNET.Entities.EntityHelpers.ToEntityType($"minecraft:{knownData.Name}");

				if (knownData.Name.Equals("bee"))
					type = (EntityType)122;
				else if (knownData.Name.Equals("fox"))
					type = (EntityType) 121;

				entity = EntityFactory.Create(type, null, type != EntityType.ArmorStand && type != EntityType.PrimedTnt);
				

				if (entity == null)
				{
					Log.Warn($"Could not map entity: {knownData.Name}");
					entity = new Entity(null);
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
			else
			{
			//	Log.Warn($"Could not create entity of type: {(int) type}:{(knownData != null ? knownData.Name : type.ToString())} (Missing entityfactory mapping...)");

				return null;
			}

			if (renderer == null)
			{
				Log.Debug($"Missing renderer for entity: {type.ToString()} ({(int) type})");

				return null;
			}
			
			if (entity.Texture == null)
			{
				
				Log.Debug($"Missing texture for entity: {type.ToString()} ({(int) type})");

				return null;
			}

			entity.ModelRenderer = renderer;

			entity.KnownPosition = position;
			entity.Velocity = velocity;
			entity.EntityId = entityId;
			entity.UUID = uuid;
			//entity.Texture = texture2D;

			if (entity is EntityArmorStand armorStand)
			{
				armorStand.IsAffectedByGravity = false;
				armorStand.NoAi = true;
			}
		//	if (!_initiated)
		//	{
		//		_entitySpawnQueue.Enqueue(entity);
		//	}
		//	else
			{
				World.SpawnEntity(entity);
			}

			return entity;
		}
		
		private void SendPacket(Packet packet)
		{
			Client.SendPacket(packet);
		}

		void IPacketHandler.HandlePlay(Packet packet)
		{
			switch (packet)
			{
				case KeepAlivePacket keepAlive:
					HandleKeepAlivePacket(keepAlive);

					break;

				case PlayerPositionAndLookPacket playerPos:
					HandlePlayerPositionAndLookPacket(playerPos);

					break;

				case ChunkDataPacket chunk:
					HandleChunkData(chunk);

					break;

				case UpdateLightPacket updateLight:
					HandleUpdateLightPacket(updateLight);

					break;

				case JoinGamePacket joinGame:
					HandleJoinGamePacket(joinGame);

					break;

				case UnloadChunk unloadChunk:
					HandleUnloadChunk(unloadChunk);

					break;

				case ChatMessagePacket chatMessage:
					HandleChatMessagePacket(chatMessage);

					break;

				case TimeUpdatePacket timeUpdate:
					HandleTimeUpdatePacket(timeUpdate);

					break;

				case PlayerAbilitiesPacket abilitiesPacket:
					HandlePlayerAbilitiesPacket(abilitiesPacket);

					break;

				case EntityPropertiesPacket entityProperties:
					HandleEntityPropertiesPacket(entityProperties);

					break;

				case EntityTeleport teleport:
					HandleEntityTeleport(teleport);

					break;

				case SpawnLivingEntity spawnMob:
					HandleSpawnMob(spawnMob);

					break;

				case SpawnEntity spawnEntity:
					HandleSpawnEntity(spawnEntity);

					break;

				case EntityLook look:
					HandleEntityLook(look);

					break;

				case EntityRelativeMove relative:
					HandleEntityRelativeMove(relative);

					break;

				case EntityLookAndRelativeMove relativeLookAndMove:
					HandleEntityLookAndRelativeMove(relativeLookAndMove);

					break;

				case PlayerListItemPacket playerList:
					HandlePlayerListItemPacket(playerList);

					break;

				case SpawnPlayerPacket spawnPlayerPacket:
					HandleSpawnPlayerPacket(spawnPlayerPacket);

					break;

				case DestroyEntitiesPacket destroy:
					HandleDestroyEntitiesPacket(destroy);

					break;

				case EntityHeadLook headlook:
					HandleEntityHeadLook(headlook);

					break;

				case FacePlayerPacket facePlayerPacket:
					HandleFacePlayer(facePlayerPacket);

					break;

				case EntityVelocity velocity:
					HandleEntityVelocity(velocity);

					break;

				case WindowItems itemsPacket:
					HandleWindowItems(itemsPacket);

					break;

				case SetSlot setSlotPacket:
					HandleSetSlot(setSlotPacket);

					break;

				case HeldItemChangePacket pack:
					HandleHeldItemChangePacket(pack);

					break;

				case EntityStatusPacket entityStatusPacket:
					HandleEntityStatusPacket(entityStatusPacket);

					break;

				case BlockChangePacket blockChangePacket:
					HandleBlockChangePacket(blockChangePacket);

					break;

				case MultiBlockChange multiBlock:
					HandleMultiBlockChange(multiBlock);

					break;

				case TabCompleteClientBound tabComplete:
					HandleTabCompleteClientBound(tabComplete);

					break;

				case ChangeGameStatePacket p:
					HandleChangeGameStatePacket(p);

					break;

				case EntityMetadataPacket entityMetadata:
					HandleEntityMetadataPacket(entityMetadata);

					break;

				case CombatEventPacket combatEventPacket:
					HandleCombatEventPacket(combatEventPacket);

					break;

				case EntityEquipmentPacket entityEquipmentPacket:
					HandleEntityEquipmentPacket(entityEquipmentPacket);

					break;

				case RespawnPacket respawnPacket:
					HandleRespawnPacket(respawnPacket);

					break;

				case TitlePacket titlePacket:
					HandleTitlePacket(titlePacket);

					break;

				case UpdateHealthPacket healthPacket:
					HandleUpdateHealthPacket(healthPacket);

					break;

				case DisconnectPacket disconnectPacket:
					HandleDisconnectPacket(disconnectPacket);

					break;

				case EntityAnimationPacket animationPacket:
					HandleAnimationPacket(animationPacket);

					break;

				case OpenWindowPacket openWindowPacket:
					HandleOpenWindowPacket(openWindowPacket);

					break;

				case CloseWindowPacket closeWindowPacket:
					HandleCloseWindowPacket(closeWindowPacket);

					break;

				case WindowConfirmationPacket confirmationPacket:
					HandleWindowConfirmationPacket(confirmationPacket);

					break;

				case SpawnPositionPacket spawnPositionPacket:
					HandleSpawnPositionPacket(spawnPositionPacket);

					break;

				case UpdateViewPositionPacket updateViewPositionPacket:
					HandleUpdateViewPositionPacket(updateViewPositionPacket);

					break;

				case UpdateViewDistancePacket viewDistancePacket:
					HandleUpdateViewDistancePacket(viewDistancePacket);

					break;

				case BlockEntityDataPacket blockEntityDataPacket:
					HandleBlockEntityData(blockEntityDataPacket);

					break;

				case BlockActionPacket blockActionPacket:
					HandleBlockAction(blockActionPacket);

					break;

				case AcknowledgePlayerDiggingPacket diggingPacket:
					HandleAcknowledgePlayerDiggingPacket(diggingPacket);

					break;

				case DisplayScoreboardPacket displayScoreboardPacket:
					HandleDisplayScoreboardPacket(displayScoreboardPacket);

					break;

				case ScoreboardObjectivePacket scoreboardObjectivePacket:
					HandleScoreboardObjectivePacket(scoreboardObjectivePacket);

					break;

				case UpdateScorePacket updateScorePacket:
					HandleUpdateScorePacket(updateScorePacket);

					break;

				case TeamsPacket teamsPacket:
					HandleTeamsPacket(teamsPacket);

					break;

				case SoundEffectPacket soundEffectPacket:
					HandleSoundEffectPacket(soundEffectPacket);

					break;
				
				case EntitySoundEffectPacket entitySoundEffectPacket:
					HandleEntitySoundEffectPacket(entitySoundEffectPacket);
					break;

				case NamedSoundEffectPacket namedSoundEffectPacket:
					HandleNamedSoundEffectPacket(namedSoundEffectPacket);
					break;
				
				case ParticlePacket particlePacket:
					HandleParticlePacket(particlePacket);

					break;

				case EntityEffectPacket effectPacket:
					HandleEntityEffectPacket(effectPacket);

					break;

				case PluginMessagePacket pluginMessagePacket:
					HandlePluginMessagePacket(pluginMessagePacket);
					break;
				
				case BossBarPacket bossBarPacket:
					HandleBossBarPacket(bossBarPacket);
					break;
				
				case SetExperiencePacket experiencePacket:
					HandleSetExperiencePacket(experiencePacket);
					break;
				
				case DeclareCommandsPacket declareCommandsPacket:
					HandleDeclareCommandsPacket(declareCommandsPacket);
					break;
				
				default:
				{
					if (UnhandledPackets.TryAdd(packet.PacketId, packet.GetType()))
					{
						Log.Warn($"Unhandled packet: 0x{packet.PacketId:x2} - {packet.ToString()}");
					}

					break;
				}
			}
		}

		private void HandleDeclareCommandsPacket(DeclareCommandsPacket packet)
		{
			var nodes = packet.Nodes.ToArray();
			var rootNode = nodes[packet.RootIndex];

			foreach (var childIndex in rootNode.Children)
			{
				var child = nodes[childIndex];

				if (child is LiteralCommandNode lcn)
				{
					var command = new Command(lcn.Name.Split(':'));

					foreach (var ci in lcn.Children)
					{
						var subChild = nodes[ci];

						if (subChild is ArgumentCommandNode acn)
						{
							foreach (var property in acn.Properties)
							{
								//CommandProperty commandProperty = new CommandProperty(property.Name, !acn.IsExecutable);
							
							
								//command.AddProperty(commandProperty);
							}
							CommandProperty commandProperty = new CommandProperty(acn.Name, !acn.IsExecutable);
							
							
							command.AddProperty(commandProperty);
						}
					}
					CommandProvider.Register(command);
				}
			}
			
			Log.Info($"Registered {CommandProvider.Count} commands.");
			//CommandProvider.Register();
		}
		
		private void HandleSetExperiencePacket(SetExperiencePacket packet)
		{
			var player = World?.Player;

			if (player == null)
				return;

			player.ExperienceLevel = packet.Level;
			player.Experience = packet.ExperienceBar;
		}
		
		private void HandleBossBarPacket(BossBarPacket packet)
		{
			var container = BossBarContainer;

			if (container == null)
				return;

			switch (packet.Action)
			{
				case BossBarPacket.BossBarAction.Add:
					container.Add(
						packet.Uuid, packet.Title, packet.Health, packet.Color, packet.Divisions, packet.Flags);
					break;

				case BossBarPacket.BossBarAction.Remove:
					container.Remove(packet.Uuid);
					break;

				case BossBarPacket.BossBarAction.UpdateHealth:
					container.UpdateHealth(packet.Uuid, packet.Health);
					break;

				case BossBarPacket.BossBarAction.UpdateTitle:
					container.UpdateTitle(packet.Uuid, packet.Title);
					break;

				case BossBarPacket.BossBarAction.UpdateStyle:
					container.UpdateStyle(packet.Uuid, packet.Color, packet.Divisions);
					break;

				case BossBarPacket.BossBarAction.UpdateFlags:
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void HandlePluginMessagePacket(PluginMessagePacket packet)
		{
			Log.Info($"Received plugin message. Channel={packet.Channel} Data={Encoding.UTF8.GetString(packet.Data)}");
		}
		
		private void HandleEntityEffectPacket(EntityEffectPacket packet)
		{
			if (World.TryGetEntity(packet.EntityId, out var entity))
			{
				Effect effect = null;

				switch (packet.EffectId)
				{
					case 1:
						effect = new SpeedEffect();
						break;
					//case 2:
						//effect = new 
					//	break;
					case 8:
						effect = new JumpBoostEffect();
						break;
					case 16:
						effect = new NightVisionEffect();
						break;
					default:
						Log.Warn($"Missing effect implementation: {packet.EffectId}");
						break;
				}

				if (effect != null)
				{
					effect.Duration = packet.Duration;
					effect.Level = packet.Amplifier;
					effect.Particles = (packet.Flags & 0x02) != 0;
					
					entity.AddOrUpdateEffect(effect);
				}
			}
		}
		
		private float RandomParticleOffset()
		{
			return 1f - (FastRandom.Instance.NextFloat() * 2f);
		}

		private void HandleParticlePacket(ParticlePacket packet)
		{
			var particleType =
				Alex.Resources.Registries.Particles.Entries.FirstOrDefault(
					x => x.Value.ProtocolId == packet.ParticleId);

			if (particleType.Key != null)
			{
				var type = ParticleConversion.ConvertToBedrock(particleType.Key);
				/*int data = 0;

				if (packet.Color.HasValue)
				{
					var color = packet.Color.Value;
					byte r = color.R;
					byte g = color.G;
					byte b = color.B;
					byte a = color.A;
					data = ((a & 0xff) << 24) | ((r & 0xff) << 16) | ((g & 0xff) << 8) | (b & 0xff);
				}*/

				for (int i = 0; i < packet.ParticleCount; i++)
				{
					long data = 0;
					ParticleDataMode dataMode = ParticleDataMode.None;
					if (packet.SlotData != null)
					{
						//var item = GetItemFromSlotData(packet.SlotData);

						//if (item != null)
						{
							dataMode = ParticleDataMode.Item;
							data = packet.SlotData.ItemID;//BlockFactory.GetBlockStateID(item.Id, (byte) item.Meta);
							//particleInstance.SetData(item.Id, ParticleDataMode.Item);
						}
					}
					else if (packet.BlockStateId.HasValue)
					{
						dataMode = ParticleDataMode.BlockRuntimeId;
						data = packet.BlockStateId.Value;
						//particleInstance.SetData(packet.BlockStateId.Value, ParticleDataMode.BlockRuntimeId);
					}
					else if (packet.Color.HasValue)
					{
						dataMode = ParticleDataMode.Color;
						data = packet.Color.Value.PackedValue;
					}
					
					if (Alex.ParticleManager.SpawnParticle(
						type,
						new Vector3(
							(float) ((float) packet.X + (packet.OffsetX * RandomParticleOffset())),
							(float) ((float) packet.Y + (packet.OffsetY * RandomParticleOffset())),
							(float) ((float) packet.Z + (packet.OffsetZ * RandomParticleOffset()))),
						out var particleInstance, data, dataMode))
					{

						//particleInstance.Scale = packet.Scale;
					}
					else
					{
						Log.Debug(
							$"Could not spawn particle with type: {packet.ParticleId} (Java: {particleType.Key} | Bedrock: {type})");

						break;
					}
				}
			}
			else
			{
				Log.Debug($"Could not find particle with protocolid: {packet.ParticleId}");
			}
		}
		
		private ThreadSafeList<string> _missingSounds = new ThreadSafeList<string>();

		private static readonly Regex _blockRegex = new Regex("block\\.(?<name>.*)\\.(?<action>.*)", RegexOptions.Compiled);

		private bool TryResolveSound(int soundId, out string name)
		{
			name = null;
			var soundEffect =
				Alex.Resources.Registries.Sounds.Entries.FirstOrDefault(x => x.Value.ProtocolId == soundId).Key;

			if (string.IsNullOrWhiteSpace(soundEffect))
				return false;

			soundEffect = soundEffect.Replace("minecraft:", "");

			var match = _blockRegex.Match(soundEffect);
		
			if (match.Success)
			{
				string action = match.Groups["action"].Value;
				switch(action)
				{
					case "break":
						action = "dig";
						break;
				}
				soundEffect = $"{action}.{match.Groups["name"].Value}";
			}
			else
			{
				/*match = _blockBreakRegex.Match(soundEffect);

				if (match.Success)
				{
					soundEffect = $"dig.{match.Groups["name"].Value}";
				}
				else
				{
					match = _blockStepRegex.Match(soundEffect);

					if (match.Success)
					{
						
					}
				}*/
				
				switch (soundEffect)
				{
					case "block.anvil.hit":
						soundEffect = "random.anvil.use";
						break;
						
					case "entity.tnt.primed":
						soundEffect = "random.fuse";
						break;
					
					case "entity.creeper.hurt":
						soundEffect = "mob.creeper.say";
						break;
					
					case "entity.firework_rocket.launch":
						soundEffect = "firework.launch";
						break;
					
					case "entity.lightning_bolt.thunder":
						soundEffect = "ambient.weather.thunder";
						break;
					case "open.chest":
						soundEffect = "random.chestopen";
						break;
					case "close.chest":
						soundEffect = "random.chestclosed";
						break;
					case "land.anvil":
						soundEffect = "random.anvil_land";
						break;
				}
			}

			name = soundEffect;

			return true;
		}

		private void HandleNamedSoundEffectPacket(NamedSoundEffectPacket packet)
		{
			if (!Alex.AudioEngine.PlaySound(packet.SoundName, packet.Position, packet.Pitch, packet.Volume))
			{
				if (_missingSounds.TryAdd(packet.SoundName))
					Log.Warn($"Missing named sound: {packet.SoundName}");
			}
		}
		
		private void HandleEntitySoundEffectPacket(EntitySoundEffectPacket packet)
		{
			if (World.TryGetEntity(packet.EntityId, out var entity))
			{
				if (TryResolveSound(packet.SoundId, out var soundEffect) && !Alex.AudioEngine.PlaySound(
					soundEffect, entity.KnownPosition, packet.Pitch, packet.Volume))
				{
					if (_missingSounds.TryAdd(soundEffect))
						Log.Warn($"Missing entity sound: {soundEffect}");
				}
			}
		}
		
		private void HandleSoundEffectPacket(SoundEffectPacket packet)
		{
			if (TryResolveSound(packet.SoundId, out var soundEffect) &&
			    !Alex.AudioEngine.PlaySound(soundEffect, packet.Position, packet.Pitch, packet.Volume))
			{
				if (_missingSounds.TryAdd(soundEffect))
					Log.Warn($"Missing sound: {soundEffect}");
			}
		}
		
		private TeamsManager TeamsManager { get; } = new TeamsManager();

		private void UpdateTeamEntry(Team team)
		{
			foreach (var entity in team.Entities)
			{
				if (ScoreboardView.TryGetEntityScoreboard(entity, out var objective))
				{
					if (objective.TryGet(entity, out var scoreboardEntry))
					{
						scoreboardEntry.DisplayName = $"{team.TeamPrefix}{entity}{team.TeamSuffix}";
					}
				}
			}
		}
		private void HandleTeamsPacket(TeamsPacket packet)
		{
			switch (packet.PacketMode)
			{
				case TeamsPacket.Mode.CreateTeam:
					if (packet.Payload is TeamsPacket.CreateTeam ct)
					{
					//	Log.Info($"Create team! Name: {packet.TeamName} Displayname: {ct.TeamDisplayName} Prefix: {ct.TeamPrefix} Suffix: {ct.TeamSuffix} Entities: {string.Join(',', ct.Entities)}");
						
						Team team = new Team(
							packet.TeamName, ct.TeamDisplayName, ct.TeamColor, ct.TeamPrefix, ct.TeamSuffix);
						
						foreach (var entity in ct.Entities)
						{
							team.AddEntity(entity);
						}
						
						TeamsManager.AddOrUpdateTeam(
							packet.TeamName,
							team);
						
						UpdateTeamEntry(team);
					}

					break;

				case TeamsPacket.Mode.RemoveTeam:
				//	Log.Info($"Remove team: {packet.TeamName}");
					TeamsManager.RemoveTeam(packet.TeamName);
					break;

				case TeamsPacket.Mode.UpdateTeam:
					if (packet.Payload is TeamsPacket.UpdateTeam ut)
					{
					//	Log.Info($"Update team! Name: {packet.TeamName} Displayname: {ut.TeamDisplayName} Prefix: {ut.TeamPrefix} Suffix: {ut.TeamSuffix}");
						if (TeamsManager.TryGet(packet.TeamName, out var team))
						{
							team.DisplayName = ut.TeamDisplayName;
							team.Color = ut.TeamColor;
							team.TeamPrefix = ut.TeamPrefix;
							team.TeamSuffix = ut.TeamSuffix;
							
							TeamsManager.AddOrUpdateTeam(packet.TeamName, team);
							UpdateTeamEntry(team);
						}
					}

					break;

				case TeamsPacket.Mode.AddPlayer:
					if (packet.Payload is TeamsPacket.AddPlayers addPlayers)
					{
						if (TeamsManager.TryGet(packet.TeamName, out var team))
						{
						//	Log.Info($"Add entities to team: Name={packet.TeamName} Entities: {string.Join(',', addPlayers.Entities)}");
							foreach (var entity in addPlayers.Entities)
							{
								team.AddEntity(entity);
							}
							
							UpdateTeamEntry(team);
						}
					}
					break;

				case TeamsPacket.Mode.RemovePlayer:
					if (packet.Payload is TeamsPacket.RemovePlayers removePlayers)
					{
						if (TeamsManager.TryGet(packet.TeamName, out var team))
						{
						//	Log.Info($"Remove entities from team: Name={packet.TeamName} Entities: {string.Join(',', removePlayers.Entities)}");
							foreach (var entity in removePlayers.Entities)
							{
								team.RemoveEntity(entity);
							}
						}
					}
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		private void HandleUpdateScorePacket(UpdateScorePacket packet)
		{
			//Log.Info($"Update score, action={packet.Action} value={packet.Value} entityname={packet.EntityName} objectiveName={packet.ObjectiveName}");
			var scoreboard = ScoreboardView;
			if (scoreboard == null)
				return;

			if (scoreboard.TryGetObjective(packet.ObjectiveName, out var obj) || scoreboard.TryGetEntityScoreboard(packet.EntityName, out obj))
			{
				if (packet.Action == UpdateScorePacket.UpdateScoreAction.CreateOrUpdate)
				{
					string displayName = packet.EntityName;
					ScoreboardEntry entry = null;

					obj.AddOrUpdate(packet.EntityName, new ScoreboardEntry(packet.EntityName, (uint) packet.Value, displayName));

					if (TeamsManager.TryGetEntityTeam(packet.EntityName, out var entityTeam))
					{
						UpdateTeamEntry(entityTeam);
					}
				}
				else if (packet.Action == UpdateScorePacket.UpdateScoreAction.Remove)
				{
				//	Log.Info($"Removed {packet.EntityName}");
					obj.Remove(packet.EntityName);
				}
			}
			else
			{
				Log.Warn($"Unknown objective: {packet.ObjectiveName}");
			}
		}

		private void HandleScoreboardObjectivePacket(ScoreboardObjectivePacket packet)
		{
		//	Log.Info($"Scoreboard objective, mode={packet.Mode} Name={packet.ObjectiveName} Value={packet.Value ?? "N/A"} Type={packet.Type}");
			var scoreboard = ScoreboardView;
			if (scoreboard == null)
				return;
			
			switch (packet.Mode)
			{
				case ScoreboardObjectivePacket.ObjectiveMode.Create:
					//packet.Type
					scoreboard.AddObjective(new ScoreboardObjective(packet.ObjectiveName, packet.Value, 1, "dummy"));
					break;

				case ScoreboardObjectivePacket.ObjectiveMode.Remove:
					scoreboard.RemoveObjective(packet.ObjectiveName);
					break;

				case ScoreboardObjectivePacket.ObjectiveMode.UpdateText:
					if (scoreboard.TryGetObjective(packet.ObjectiveName, out var objective))
					{
						objective.DisplayName = packet.Value;
					}
					break;
			}
			//packet.
		}

		private void HandleDisplayScoreboardPacket(DisplayScoreboardPacket packet)
		{
		//	Log.Info($"Display scoreboard: {packet.ScoreName} Position: {packet.Position}");
			if (packet.Position == DisplayScoreboardPacket.ScoreboardPosition.Sidebar)
			{
				var scoreboard = ScoreboardView;
				if (scoreboard == null)
					return;
				
			
			}
		}

		private void HandleAcknowledgePlayerDiggingPacket(AcknowledgePlayerDiggingPacket packet)
		{
			Log.Info($"Player digging acknowledgement, status={packet.Status} success={packet.Successful}");
			
			if (!packet.Successful)
			{
				World.SetBlockState(packet.Position, BlockFactory.GetBlockState((uint)packet.Block));
			}
		}

		private void HandleBlockAction(BlockActionPacket packet)
		{
			if (World.EntityManager.TryGetBlockEntity(packet.Location, out BlockEntity entity))
			{
				entity.HandleBlockAction(packet.ActionId, packet.Parameter);
			}
		}

		private void HandleUpdateViewDistancePacket(UpdateViewDistancePacket packet)
		{
			//World.ChunkManager.RenderDistance = packet.ViewDistance / 16;
		}

		private bool _gotViewPosition = false;
		private void HandleUpdateViewPositionPacket(UpdateViewPositionPacket packet)
		{
			_gotViewPosition = true;
			//	World.ChunkManager.ViewPosition = new ChunkCoordinates(packet.ChunkX, packet.ChunkZ);
		}

		private void HandleSpawnPositionPacket(SpawnPositionPacket packet)
		{
			_spawn = packet.SpawnPosition;
			HasSpawnPosition = true;
		}
		
		private void InventoryOnCursorChanged(object sender, CursorChangedEventArgs e)
		{
			if (e.IsServerTransaction)
				return;
			
			if (sender is InventoryBase inv)
			{
				ClickWindowPacket.TransactionMode mode = ClickWindowPacket.TransactionMode.Click;
				byte button = 0;
				switch (e.Button)
				{
					case MouseButton.Left:
						button = 0;
						break;
					case MouseButton.Right:
						button = 1;
						break;
				}
				
				/*if (e.Value.Id <= 0 || e.Value is ItemAir)
				{
					e.Value.Id = -1;
					mode = ClickWindowPacket.TransactionMode.Drop;
				}*/

				short actionNumber = (short) inv.ActionNumber++;

				ClickWindowPacket packet = ClickWindowPacket.CreateObject();
				packet.Mode = mode;
				packet.Button = button;
				packet.Action = actionNumber;
				packet.WindowId = (byte) inv.InventoryId;
				packet.Slot = (short) e.Index;
				packet.ClickedItem = new SlotData()
				{
					Count = (byte) e.Value.Count,
					Nbt = e.Value.Nbt,
					ItemID = e.Value.Id
				};
				
				inv.UnconfirmedWindowTransactions.TryAdd(actionNumber, (packet, e, true));
				Client.SendPacket(packet);
				
				Log.Info($"Sent transaction with id: {actionNumber} Item: {e.Value.Id} Mode: {mode}");
			}
		}

		private void InventoryOnSlotChanged(object sender, SlotChangedEventArgs e)
		{
			if (e.IsServerTransaction)
				return;
			
			
		}

		private void HandleWindowConfirmationPacket(WindowConfirmationPacket packet)
		{
			InventoryBase inventory = null;
			if (packet.WindowId == 0)
			{
				inventory = World.Player.Inventory;
			}
			else
			{
				if (World.InventoryManager.TryGet(packet.WindowId, out var gui))
				{
					inventory = gui.Inventory;
				}
			}

			if (!packet.Accepted)
			{
			//	Log.Warn($"Inventory / window transaction has been denied! (Action: {packet.ActionNumber})");
				
				WindowConfirmationPacket response = WindowConfirmationPacket.CreateObject();
				response.Accepted = false;
				response.ActionNumber = packet.ActionNumber;
				response.WindowId = packet.WindowId;
				
				Client.SendPacket(response);
			}
			else
			{
			//	Log.Info($"Transaction got accepted! (Action: {packet.ActionNumber})");
			}

			if (inventory == null)
				return;

			if (inventory.UnconfirmedWindowTransactions.TryGetValue(packet.ActionNumber, out var transaction))
			{
				inventory.UnconfirmedWindowTransactions.Remove(packet.ActionNumber);

				if (!packet.Accepted)
				{
					//if (transaction.isCursorTransaction)
					{
						
					}
					//else
					{
						inventory.SetSlot(transaction.packet.Slot,
							GetItemFromSlotData(transaction.packet.ClickedItem), true);
					}
				}
			}
		}

		private void HandleCloseWindowPacket(CloseWindowPacket packet)
		{
			World.InventoryManager.Close(packet.WindowId);
		}
		
		private void HandleOpenWindowPacket(OpenWindowPacket packet)
		{
			GuiInventoryBase inventoryBase = null;
			switch (packet.WindowType)
			{
				//Chest
				case 2:
					inventoryBase = World.InventoryManager.Show(World.Player.Inventory, packet.WindowId, ContainerType.Chest);
					break;
				
				//Large Chest:
				case 5:
					inventoryBase = World.InventoryManager.Show(World.Player.Inventory, packet.WindowId, ContainerType.Chest);
					break;
			}

			if (inventoryBase == null)
				return;

			inventoryBase.Inventory.CursorChanged += InventoryOnCursorChanged;
			inventoryBase.Inventory.SlotChanged += InventoryOnSlotChanged;
			inventoryBase.OnContainerClose += (sender, args) =>
			{
				inventoryBase.Inventory.CursorChanged -= InventoryOnCursorChanged;
				inventoryBase.Inventory.SlotChanged -= InventoryOnSlotChanged;
				ClosedContainer((byte) inventoryBase.Inventory.InventoryId);
			};
		}

		private void ClosedContainer(byte containerId)
		{
			CloseWindowPacket packet = CloseWindowPacket.CreateObject();
			packet.WindowId = containerId;
			Client.SendPacket(packet);
		}
		
		private void HandleAnimationPacket(EntityAnimationPacket packet)
		{
			if (World.TryGetEntity(packet.EntityId, out Entity entity))
			{
				switch (packet.Animation)
				{
					case EntityAnimationPacket.Animations.SwingMainArm:
						entity.SwingArm(false, false);
						break;

					case EntityAnimationPacket.Animations.TakeDamage:
						entity.EntityHurt();
						break;

					case EntityAnimationPacket.Animations.LeaveBed:
						break;

					case EntityAnimationPacket.Animations.SwingOffhand:
						entity.SwingArm(false, true);
						break;

					case EntityAnimationPacket.Animations.CriticalEffect:
						break;

					case EntityAnimationPacket.Animations.MagicCriticalEffect:
						break;
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
					TitleComponent.Show();
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

		private void HandleDimension(NbtCompound dim)
		{
			Dimension dimension = Dimension.Overworld;
			switch (dim["effects"]?.StringValue)
			{
				case "minecraft:the_nether":
					dimension = Dimension.Nether;
					break;
				case "minecraft:overworld":
					dimension = Dimension.Overworld;
					break;
				case "minecraft:the_end":
					dimension = Dimension.TheEnd;
					break;
				default:
					Log.Warn($"Unknown dimension: {dim}");
					break;
			}

			World.Dimension = dimension;

			var fixedTime = dim["fixed_time"];

			if (fixedTime != null && fixedTime.HasValue)
			{
				if (fixedTime.TagType == NbtTagType.Long)
				{
					World.SetGameRule(GameRulesEnum.DoDaylightcycle, false);
					World.SetTime(World.Time, fixedTime.LongValue);
				}
			}
			else
			{
				World.SetGameRule(GameRulesEnum.DoDaylightcycle, true);
			}

			var hasSkyLight = dim["has_skylight"];

			if (hasSkyLight != null && hasSkyLight.HasValue)
			{
				if (hasSkyLight.ByteValue == 1)
				{
					
				}
			}
		}
		
		private void HandleRespawnPacket(RespawnPacket packet)
		{

			Respawning = true;
			
			HandleDimension(packet.Dimension);
			
			World.Player.UpdateGamemode(packet.Gamemode);
			World.ClearChunksAndEntities();
			
			SendPlayerPositionAndLook(World.Player.KnownPosition, SendPositionReason.Respawn);
			
			//player.


			/*new Thread(() =>
			{
				LoadingWorldState state = new LoadingWorldState();
				state.UpdateProgress(LoadingState.LoadingChunks, 0);
				Alex.GameStateManager.SetActiveState(state, true);

				int t = Options.VideoOptions.RenderDistance;
				double radiusSquared = Math.Pow(t, 2);

				var target = radiusSquared * 3;

				while (Respawning)
				{
					int chunkProgress = (int) Math.Floor((target / 100) * World.ChunkManager.ChunkCount);
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
			}).Start();*/
		}

		public static Item GetItemFromSlotData(SlotData data)
		{
			if (data == null)
				return new ItemAir();
			
			if (ItemFactory.ResolveItemName(data.ItemID, out string name))
			{
				if (ItemFactory.TryGetItem(name, out Item item))
				{
					item = item.Clone();
					
					item.Id = (short) data.ItemID;
					item.Count = data.Count;
					item.Nbt = data.Nbt;

					return item;
				}
			}

			return new ItemAir();
		}

		private void HandleEntityEquipmentPacket(EntityEquipmentPacket packet)
		{
			/*if (packet.Item == null)
			{
				Log.Warn($"Got null item in EntityEquipment.");
				return;
			}*/

			if (World.TryGetEntity(packet.EntityId, out Entity entity))
			{
				{
					Item item = GetItemFromSlotData(packet.Item).Clone();;

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
			if (World.TryGetEntity(packet.EntityId, out var entity))
			{
				packet.FinishReading();
				foreach (var entry in packet.Entries)
				{
					entity.HandleJavaMetadata(entry);
				}
			}
		}

		private void HandleEntityStatusPacket(EntityStatusPacket packet)
		{
			//
			if (World.TryGetEntity(packet.EntityId, out var entity))
			{
				entity.HandleEntityStatus(packet.EntityStatus);
			}
		}

		private void HandleCombatEventPacket(CombatEventPacket packet)
		{
			if (packet.Event == CombatEventPacket.CombatEvent.EntityDead)
			{
				Log.Warn($"Status packet: Entity={packet.EntityId} Player={packet.PlayerId} Message={packet.Message}");
				ClientStatusPacket statusPacket = ClientStatusPacket.CreateObject();
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
					World?.Player?.UpdateGamemode((GameMode) packet.Value);
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
			CommandProvider.HandleTabCompleteClientBound(tabComplete);
			//TODO: Re-implement tab complete
		//	Log.Info($"!!! TODO: Re-implement tab complete.");
			//ChatReceiver?.ReceivedTabComplete(tabComplete.TransactionId, tabComplete.Start, tabComplete.Length, tabComplete.Matches);
		}

		private void HandleMultiBlockChange(MultiBlockChange packet)
		{
			foreach (var blockUpdate in packet.Records)
			{
				World?.SetBlockState(
					new BlockCoordinates(blockUpdate.X, blockUpdate.Y, blockUpdate.Z), 
					BlockFactory.GetBlockState(blockUpdate.BlockId),
					BlockUpdatePriority.High | BlockUpdatePriority.Network  | BlockUpdatePriority.Neighbors);
			}
		}

		private void HandleBlockChangePacket(BlockChangePacket packet)
		{
			//throw new NotImplementedException();
			World?.SetBlockState(packet.Location, BlockFactory.GetBlockState(packet.PalleteId), 
				BlockUpdatePriority.High | BlockUpdatePriority.Network | BlockUpdatePriority.Neighbors);
		}

		private void HandleHeldItemChangePacket(HeldItemChangePacket packet)
		{
			World.Player.Inventory.SelectedSlot = packet.Slot;
		}

		private void HandleSetSlot(SetSlot packet)
		{
			InventoryBase inventory = null;
			if (packet.WindowId == 0 || packet.WindowId == -2)
			{
				inventory = World.Player.Inventory;
			}
			else if (packet.WindowId == -1)
			{
				var active = World.InventoryManager.ActiveWindow;
				if (active != null)
				{
					inventory = active.Inventory;
				}
			}
			else
			{
				if (World.InventoryManager.TryGet(packet.WindowId, out GuiInventoryBase gui))
				{
					inventory = gui.Inventory;
				}
			}

			if (inventory == null) return;

			if (packet.WindowId == -1 && packet.SlotId == -1) //Set cursor
			{
				inventory.SetCursor(GetItemFromSlotData(packet.Slot), true);
			} 
			else if (packet.SlotId < inventory.SlotCount)
			{
				inventory.SetSlot(packet.SlotId, GetItemFromSlotData(packet.Slot), true);
				//inventory[packet.SlotId] = GetItemFromSlotData(packet.Slot);
			}
		}

		private void HandleWindowItems(WindowItems packet)
		{
			InventoryBase inventory = null;
			if (packet.WindowId == 0)
			{
				inventory = World.Player.Inventory;
			}
			else
			{
				if (World.InventoryManager.TryGet(packet.WindowId, out GuiInventoryBase gui))
				{
					inventory = gui.Inventory;
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

					var item = GetItemFromSlotData(packet.Slots[i]);

					if (item != null)
					{
						inventory.SetSlot(i, item, true);
					}
					else
					{
						
					}

					//inventory[i] = GetItemFromSlotData(packet.Slots[i]);
				}
			}
		}

		private void HandleDestroyEntitiesPacket(DestroyEntitiesPacket packet)
		{
			foreach(var id in packet.EntityIds)
			{
				/*var p = _players.ToArray().FirstOrDefault(x => x.Value.EntityId == id);
				if (p.Key != null)
				{
					_players.TryRemove(p.Key, out _);
				}*/

				World.DespawnEntity(id);
			}
		}

		private void HandleSpawnPlayerPacket(SpawnPlayerPacket packet)
		{
			if (_players.TryGetValue(packet.Uuid, out var entry))
			{
				RemotePlayer entity = new RemotePlayer(
					World, "geometry.humanoid.custom");
				
				entity.UpdateGamemode((GameMode) entry.Gamemode);
				entity.UUID = packet.Uuid;
					
				if (entry.HasDisplayName)
				{
					if (ChatObject.TryParse(entry.DisplayName, out string chat))
					{
						entity.NameTag = chat;
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
				float yaw = MathUtils.AngleToNotchianDegree(packet.Yaw);
				entity.KnownPosition = new PlayerLocation(packet.X, packet.Y, packet.Z, yaw, yaw, -MathUtils.AngleToNotchianDegree(packet.Pitch));
				entity.EntityId = packet.EntityId;

				if (World.SpawnEntity(entity))
				{
					string skinJson = null;

					foreach (var property in entry.Properties)
					{
						if (property.Name == "textures")
						{
							skinJson = Encoding.UTF8.GetString(Convert.FromBase64String(property.Value));
						}
					}

					ProcessSkin(entity, skinJson);
				}
			}
		}

		private ConcurrentDictionary<MiNET.Utils.UUID, PlayerListItemPacket.AddPlayerEntry> _players = new ConcurrentDictionary<MiNET.Utils.UUID, PlayerListItemPacket.AddPlayerEntry>();
		private void HandlePlayerListItemPacket(PlayerListItemPacket packet)
		{
			List<Action> actions = new List<Action>();
			if (packet.Action == PlayerListAction.AddPlayer)
			{
				foreach (var entry in packet.AddPlayerEntries)
				{
					var uuid = entry.UUID;

					if (_players.TryAdd(uuid, entry))
					{
						World.AddPlayerListItem(
							new PlayerListItem(uuid, entry.Name, (GameMode) entry.Gamemode, entry.Ping, true));
					}
				}
			}
			else if (packet.Action == PlayerListAction.UpdateLatency)
			{
				foreach (var entry in packet.UpdateLatencyEntries)
				{
					var uuid = entry.UUID;
					
					World?.UpdatePlayerLatency(uuid, entry.Ping);
				}
			}
			else if (packet.Action == PlayerListAction.UpdateDisplayName)
			{
				foreach (var entry in packet.UpdateDisplayNameEntries)
				{
					var uuid = entry.UUID;

					if (World.EntityManager.TryGet(uuid, out var entity))
					{
						if (entry.HasDisplayName && !string.IsNullOrWhiteSpace(entry.DisplayName))
						{
							if (ChatObject.TryParse(entry.DisplayName, out string chat))
							{
								entity.NameTag = chat;
							}
							else
							{
								entity.NameTag = entry.DisplayName;
							}
						}
						else
						{
							//entity.NameTag = entity.Name;
						}
						
						World?.UpdatePlayerListDisplayName(uuid, entity.NameTag);
					}
				}
			}

			else if (packet.Action == PlayerListAction.RemovePlayer)
			{
				foreach (var remove in packet.RemovePlayerEntries)
				{
					var uuid = remove.UUID;
					World?.RemovePlayerListItem(uuid);
					_players.TryRemove(uuid, out _);
				}
			}
		}

		private void ProcessSkin(RemotePlayer entity, string skinJson)
		{
			if (string.IsNullOrWhiteSpace(skinJson))
			{
				Log.Warn($"Invalid java skin, skinJson was null or whitespace.");
				return;
			}
			World.BackgroundWorker.Enqueue(
				() =>
				{
					SkinUtils.TryGetSkin(skinJson, Alex.GraphicsDevice, (texture, slim) =>
					{
						if (texture != null)
						{
							var geometryName = slim ? "geometry.humanoid.customSlim" : "geometry.humanoid.custom";

							if (ModelFactory.TryGetModel(geometryName, out var entityModel))
							{
								var skin = entityModel.ToSkin();
								skin.UpdateTexture(texture);
								entity.Skin = skin;
							}
							//entity.UpdateSkin(skin);
						}
					});
				});
		}

		private void HandleEntityLookAndRelativeMove(EntityLookAndRelativeMove packet)
		{
			if (packet.EntityId == World.Player.EntityId)
				return;

			//if (World.TryGetEntity(packet.EntityId, out var entity))
			{
			//	var     currentPosition = entity.KnownPosition;
				//currentPosition.X 
				var yaw = MathUtils.AngleToNotchianDegree(packet.Yaw);

				World.UpdateEntityPosition(
					packet.EntityId,
					new PlayerLocation(
						MathUtils.FromFixedPoint(packet.DeltaX), MathUtils.FromFixedPoint(packet.DeltaY),
						MathUtils.FromFixedPoint(packet.DeltaZ), -yaw, -yaw,
						-MathUtils.AngleToNotchianDegree(packet.Pitch)) {OnGround = packet.OnGround}, true, true, true);
			}
		}

		private void HandleEntityRelativeMove(EntityRelativeMove packet)
		{
			if (packet.EntityId == World.Player.EntityId)
				return;
			
			World.UpdateEntityPosition(packet.EntityId, new PlayerLocation(MathUtils.FromFixedPoint(packet.DeltaX), MathUtils.FromFixedPoint(packet.DeltaY), MathUtils.FromFixedPoint(packet.DeltaZ))
			{
				OnGround = packet.OnGround
			}, true);
		}

		private void HandleFacePlayer(FacePlayerPacket packet)
		{
			bool    isEntity       = packet.IsEntity;
			Vector3 targetPosition = packet.Target;
			if (isEntity)
			{
				if (World.TryGetEntity(packet.EntityId, out var entity))
				{
					targetPosition = entity.RenderLocation.ToVector3();

					if (packet.LookAtEyes)
					{
						targetPosition.Y += (float)entity.Height;
					}
				}
			}

			World.Player.LookAt(targetPosition, packet.AimWithHead);
		}

		private void HandleEntityHeadLook(EntityHeadLook packet)
		{
			if (packet.EntityId == World.Player.EntityId)
				return;
			
			if (World.TryGetEntity(packet.EntityId, out var entity))
			{
				entity.KnownPosition.HeadYaw = -MathUtils.AngleToNotchianDegree(packet.HeadYaw);
				//entity.UpdateHeadYaw(MathUtils.AngleToNotchianDegree(packet.HeadYaw));
			}
		}

		private void HandleEntityLook(EntityLook packet)
		{
			if (packet.EntityId == World.Player.EntityId)
				return;
			
			World.UpdateEntityLook(packet.EntityId, -MathUtils.AngleToNotchianDegree(packet.Yaw), -MathUtils.AngleToNotchianDegree(packet.Pitch), packet.OnGround);
		}

		private void HandleEntityTeleport(EntityTeleport packet)
		{
			if (packet.EntityID == World.Player.EntityId)
				return;
			
			float yaw = MathUtils.AngleToNotchianDegree(packet.Yaw);
			World.UpdateEntityPosition(packet.EntityID, new PlayerLocation(packet.X, packet.Y, packet.Z, -yaw, -yaw, -MathUtils.AngleToNotchianDegree(packet.Pitch))
			{
				OnGround = packet.OnGround
			}, updateLook: true, updatePitch:true, relative:false, teleport:true);
		}
		
		private Vector3 ModifyVelocity(Vector3 velocity)
		{
			return velocity / 8000f;
		}
		
		private void HandleEntityVelocity(EntityVelocity packet)
		{
			Entity entity = null;
			
			if (packet.EntityId == World.Player.EntityId)
			{
				entity = World.Player;
			}
			else if (!World.EntityManager.TryGet(packet.EntityId, out entity))
			{
				//Log.Warn($"Unkown entity in EntityVelocity: {packet.EntityId}");

				return;
			}

			if (entity != null)
			{
				var velocity = ModifyVelocity(new Vector3(
					packet.VelocityX, packet.VelocityY, packet.VelocityZ));

				//var old = entity.Velocity;

				entity.Movement.Velocity(velocity);
			}
		}

		private void HandleEntityPropertiesPacket(EntityPropertiesPacket packet)
		{
			Entity target;

			if (packet.EntityId == World.Player.EntityId)
			{
				target = World.Player;
			}
			else if (!World.EntityManager.TryGet(packet.EntityId, out target))
			{
				return;
			}
			
			foreach (var prop in packet.Properties.Values)
			{
				target.AddOrUpdateProperty(prop);
			}
		}

		private void HandlePlayerAbilitiesPacket(PlayerAbilitiesPacket packet)
		{
			var flags = packet.Flags;
			var player = World.Player;
			
			player.FlyingSpeed = packet.FlyingSpeed;
			
			//player.AddOrUpdateProperty();

			//player.FlyingSpeed = packet.FlyingSpeed * 10f;
			player.FOVModifier = packet.FiedOfViewModifier;
			//World.Camera.
			//player.MovementSpeed = packet.WalkingSpeed;

			player.CanFly = (flags & 0x04) != 0; //CanFly
			player.Invulnerable = (flags & 0x01) != 0; //InVulnerable

			if ((flags & 0x02) != 0) //Flying
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

		private void HandleTimeUpdatePacket(TimeUpdatePacket packet)
		{
			World.SetTime(packet.WorldAge, packet.TimeOfDay);
		}

		private void HandleChatMessagePacket(ChatMessagePacket packet)
		{
			if (ChatObject.TryParse(packet.Message, out string chat))
			{
				MessageType msgType = MessageType.Chat;
				switch (packet.Position)
				{
					case 0:
						msgType = MessageType.Chat;
						break;
					case 1:
						msgType = MessageType.System;
						break;
					case 2:
						msgType = MessageType.Popup;
						break;
				}
				
				ChatRecipient?.AddMessage(chat, msgType);

				//EventDispatcher.DispatchEvent(new ChatMessageReceivedEvent(chat, msgType));
			}
			else
			{
				Log.Warn($"Failed to parse chat object, received json: {packet.Message}");
			}
		}

		private void HandleUnloadChunk(UnloadChunk packet)
		{
			World.UnloadChunk(new ChunkCoordinates(packet.X, packet.Z));
		}

		private void HandleJoinGamePacket(JoinGamePacket packet)
		{
			//_dimension = packet.Dimension;

			SendSettings();
			
			//World.ChunkManager.RenderDistance = packet.ViewDistance / 16;
			
			World.Player.EntityId = packet.EntityId;
			World.Player.UpdateGamemode((GameMode) packet.Gamemode);
			
			HandleDimension(packet.Dimension);
		}

		private void HandleUpdateLightPacket(UpdateLightPacket packet)
		{
			return;
			
        }

        //private BlockingCollection<ChunkDataPacket> _chunkQueue = new BlockingCollection<ChunkDataPacket>();
        private void HandleChunkData(ChunkDataPacket chunk)
        {
	      World.BackgroundWorker.Enqueue(() =>
		        {
			        unsafe
			        {
				        _loginCompleteEvent?.Set();

				        //_chunkQueue.Add(chunk);
				        //	ThreadPool.QueueUserWorkItem(() =>
				        using (var memoryStream = new UnmanagedMemoryStream(
						        (byte*) chunk.Buffer.Pin().Pointer, chunk.Buffer.Length))
					        // using(var memoryStream = new MemoryStream(chunk.Buffer))
				        using (var stream = new MinecraftStream(memoryStream))
				        {
					        JavaChunkColumn result = null; // = new ChunkColumn();

					        if (chunk.GroundUp)
					        {
						        result = new JavaChunkColumn(chunk.ChunkX, chunk.ChunkZ);
					        }
					        else
					        {
						        if (World.GetChunkColumn(chunk.ChunkX, chunk.ChunkZ) is JavaChunkColumn c)
						        {
							        result = c;
						        }
						        else
						        {
							        result = new JavaChunkColumn(chunk.ChunkX, chunk.ChunkZ);
						        }
					        }

					        // result.X = chunk.ChunkX;
					        // result.Z = chunk.ChunkZ;
					        //     result.IsDirty = true;

					        result.Read(stream, chunk.PrimaryBitmask, chunk.GroundUp, World.Dimension == Dimension.Overworld);

					        if (chunk.GroundUp)
					        {
						        /*for (int i = 0; i < chunk.Biomes.Length; i++)
					        {
						        result.BiomeId[i] = chunk.Biomes[i];
					        }*/
						        for (int x = 0; x < 16; x++)
						        {
							        for (int z = 0; z < 16; z++)
							        {
								        for (int y = 0; y < 256; y++)
								        {
									        result.SetBiome(x, y, z, chunk.Biomes[((y >> 2) & 63) << 4 | ((z >> 2) & 3) << 2 | ((x >> 2) & 3)]);
								        }
							        }
						        }
					        }


					        foreach (var tag in chunk.TileEntities)
					        {
						        if (tag == null || !(tag.Contains("id")))
							        continue;
					        
						        try
						        {
							        var blockEntity = BlockEntityFactory.ReadFrom(tag, World, null);

							        if (blockEntity != null)
							        {
								        result.AddBlockEntity(
									        new BlockCoordinates(blockEntity.X, blockEntity.Y, blockEntity.Z), tag);
							        }
							        else
							        {
								        Log.Debug($"Got null block entity: {tag}");
							        }

						        }
						        catch (Exception ex)
						        {
							        Log.Warn(ex, "Could not add block entity!");
						        }
					        }

					        if (!_generatingHelper.IsAddingCompleted)
					        {
						        _generatingHelper.Add(result);
						        _chunksReceived++;

						        return;
					        }

					        World.ChunkManager.AddChunk(result, new ChunkCoordinates(result.X, result.Z), true);
				        } //);
			        }
		        });
        }

        private void HandleBlockEntityData(BlockEntityDataPacket packet)
        {
	        World.BackgroundWorker.Enqueue(
		        () =>
		        {
			        //Log.Warn($"Got block entity data for ({packet.Location}) Action={packet.Action}");

			        if (World.EntityManager.TryGetBlockEntity(packet.Location, out var entity))
			        {
				       // var block = World.GetBlockState(packet.Location);
				       // entity.Block = block.Block;

				        entity.SetData(packet.Action, packet.Compound);
			        }
			        else
			        {
				        try
				        {
					        var block       = World.GetBlockState(packet.Location);
					        var blockEntity = BlockEntityFactory.ReadFrom(packet.Compound, World, block.Block);

					        if (blockEntity != null)
					        {
						        World.SetBlockEntity(
							        packet.Location.X, packet.Location.Y, packet.Location.Z, blockEntity);

						        //Log.Info($"Added block entity of type \"{blockEntity.GetType()}\" ({packet.Location})");
					        }
				        }
				        catch (Exception ex)
				        {
					        Log.Warn(ex, $"Could not add block entity: {packet.Compound.ToString()}");
				        }
			        }
		        });
        }

        private void HandleKeepAlivePacket(KeepAlivePacket packet)
        {
	        //Log.Info($"Keep alive: {packet.KeepAliveid}");
	        KeepAlivePacket response =  KeepAlivePacket.CreateObject();
			response.KeepAliveid = packet.KeepAliveid;
			//response.PacketId = 0x0E;

			SendPacket(response);
		}

        public bool ReadyToSpawn { get; set; } = false;
        private bool HasSpawnPosition { get; set; } = false;
        private void HandlePlayerPositionAndLookPacket(PlayerPositionAndLookPacket packet)
		{
			Respawning = false;
			var x = (float)packet.X;
			var y = (float)packet.Y;
			var z = (float)packet.Z;

			var yaw = packet.Yaw;
			var pitch = packet.Pitch;
			
			var flags = packet.Flags;
			if ((flags & 0x01) != 0)
			{
				x = World.Player.KnownPosition.X + x;
			}
			
			if ((flags & 0x02) != 0)
			{
				y = World.Player.KnownPosition.Y + y;
			}
			
			if ((flags & 0x04) != 0)
			{
				z = World.Player.KnownPosition.Z + z;
			}
			
			if ((flags & 0x08) != 0)
			{
				pitch = World.Player.KnownPosition.Pitch + pitch;
			}
			
			if ((flags & 0x10) != 0)
			{
				yaw = World.Player.KnownPosition.Yaw + yaw;
			}

			World.UpdatePlayerPosition(
				new PlayerLocation()
				{
					X = x,
					Y = y,
					Z = z,
					Yaw = yaw,
					HeadYaw = yaw,
					Pitch = pitch
				});

			// if (World.Player.IsSpawned)
			//{
				TeleportConfirm confirmation = TeleportConfirm.CreateObject();
				confirmation.TeleportId = packet.TeleportId;
				SendPacket(confirmation);
			//}

			//UpdatePlayerPosition(
			//	new PlayerLocation(packet.X, packet.Y, packet.Z, packet.Yaw, packet.Yaw, pitch: packet.Pitch));

			if (!ReadyToSpawn && HasSpawnPosition)
			{
				Log.Info($"Ready to spawn!");
				
				SendPlayerPositionAndLook(World.Player.KnownPosition, SendPositionReason.Server);
				
				ClientStatusPacket clientStatus = ClientStatusPacket.CreateObject();
				clientStatus.ActionID = ClientStatusPacket.Action.PerformRespawnOrConfirmLogin;
				SendPacket(clientStatus);
				
				//World.Player.IsSpawned = true;
				ReadyToSpawn = true;
			}
		}

		void IPacketHandler.HandleHandshake(Packet packet)
		{

		}

		void IPacketHandler.HandleStatus(Packet packet)
		{

		}

		void IPacketHandler.HandleLogin(Packet packet)
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

		private void HandleSpawnEntity(SpawnEntity packet)
		{
			//World.BackgroundWorker.Enqueue(
			//	() =>
			//	{
					var velocity = Vector3.Zero;

					if (packet.Data > 0)
					{
						velocity = ModifyVelocity(new Vector3(packet.VelocityX, packet.VelocityY, packet.VelocityZ));
					}

					var mob = SpawnMob(
						packet.EntityId, packet.Uuid, (EntityType) packet.Type, new PlayerLocation(
							packet.X, packet.Y, packet.Z, packet.Yaw, packet.Yaw, packet.Pitch)
						{
							//	OnGround = packet.SpawnMob
						}, velocity);

					
					if (mob is EntityFallingBlock efb)
					{
						//32
						var blockId  = packet.Data << 12 >> 12;
						var metaData = packet.Data >> 12;

						if (ItemFactory.TryGetItem((short) blockId, (short) metaData, out var item))
						{
							efb.SetItem(item);
						}
						else { }
					}
				//});
		}

		private void HandleSpawnMob(SpawnLivingEntity packet)
		{
			/*World.BackgroundWorker.Enqueue(
				() =>
				{*/
					SpawnMob(
						packet.EntityId, packet.Uuid, (EntityType) packet.Type, new PlayerLocation(
							packet.X, packet.Y, packet.Z, packet.Yaw, packet.Yaw, packet.Pitch)
						{
							//	OnGround = packet.SpawnMob
						}, ModifyVelocity(new Vector3(packet.VelocityX, packet.VelocityY, packet.VelocityZ)));
			//	});
		}

		private void HandleDisconnectPacket(DisconnectPacket packet)
		{
			Log.Info($"Received disconnect: {packet.Message}");
			if (ChatObject.TryParse(packet.Message, out string o))
			{
				ShowDisconnect(o, force:true);
			}
			else
			{
				ShowDisconnect(packet.Message, false, true);
			}

			_disconnected = true;
			Client.Stop();
		}
		
		private void HandleLoginSuccess(LoginSuccessPacket packet)
		{
			Log.Info($"Login success! Username: {packet.Username}");
			Client.ConnectionState = ConnectionState.Play;
			
			//Client.UsePacketHandlerQueue = true;
		}

		private void HandleSetCompression(SetCompressionPacket packet)
		{
			Client.CompressionThreshold = packet.Threshold;
			Client.CompressionEnabled = packet.Threshold > 0;
			
			Log.Info($"Compression threshold: {packet.Threshold}");
		}

		private async Task<bool> VerifySession(string serverHash)
		{
			if (string.IsNullOrWhiteSpace(_accesToken))
			{
				return false;
			}
			
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
					await newStream.WriteAsync(bytes, 0, bytes.Length);
				}

				await http.GetResponseAsync();

				return true;
				/*using (var stream = r.GetResponseStream())
				using (var sr = new StreamReader(stream))
				{
					var content = await sr.ReadToEndAsync();
				}*/
			}
			catch
			{
				return false;
			}
		}
		
		private string _accesToken = "";
		private string _uuid = "";
		private string _username = "";
		private byte[] SharedSecret = new byte[16];
		private void HandleEncryptionRequest(EncryptionRequestPacket packet)
		{
			Log.Info($"Received encryption request.");
			FastRandom.Instance.NextBytes(SharedSecret);

			var serverId = packet.ServerId;
			var publicKey = packet.PublicKey;
			var verificationToken = packet.VerifyToken;
			
			string serverHash;

			using (MemoryStream ms = new MemoryStream())
			{
				byte[] ascii = Encoding.ASCII.GetBytes(serverId);
				ms.Write(ascii, 0, ascii.Length);
				ms.Write(SharedSecret, 0, 16);
				ms.Write(publicKey, 0, publicKey.Length);

				serverHash = JavaHexDigest(ms.ToArray());
			}
			
			VerifySession(serverHash).ContinueWith(
				x =>
				{
					var authenticated = x.Result;

					if (!authenticated)
					{
						ShowDisconnect("disconnect.loginFailedInfo.invalidSession", true);
						return;
					}
					
					var cryptoProvider = RsaHelper.DecodePublicKey(publicKey);
					//Log.Info($"Crypto: {cryptoProvider == null} Pub: {packet.PublicKey} Shared: {SharedSecret}");
					var encrypted = cryptoProvider.Encrypt(SharedSecret, RSAEncryptionPadding.Pkcs1);

					EncryptionResponsePacket response = EncryptionResponsePacket.CreateObject();
					response.SharedSecret = encrypted;
					response.VerifyToken = cryptoProvider.Encrypt(verificationToken, RSAEncryptionPadding.Pkcs1);
					
					Client.InitEncryption(SharedSecret);
					
					Client.SendPacket(response);// SendPacket(response);
				});//.Wait();
		}

		private bool Login(string username, string uuid, string accessToken)
		{
			try
			{
				//	_loginCompleteEvent = signalWhenReady;
				_username = username;
				_uuid = uuid;
				_accesToken = accessToken;

			//	TcpClient.Connect(Endpoint);
			
					//TcpClient.Connect(Endpoint);
				//	ServerBound.InitEncryption();

				HandshakePacket handshake = HandshakePacket.CreateObject();
				handshake.NextState = ConnectionState.Login;
				handshake.ServerAddress = Hostname;
				handshake.ServerPort = (ushort) Endpoint.Port;
				handshake.ProtocolVersion = JavaProtocol.ProtocolVersion;
				SendPacket(handshake);

				Client.ConnectionState = ConnectionState.Login;

				LoginStartPacket loginStart = LoginStartPacket.CreateObject();
				loginStart.Username = _username;
				SendPacket(loginStart);
			}
			catch (SocketException ex)
			{
				Log.Warn(ex, "Error while connecting to server.");
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
			var hash = new SHA1Managed().ComputeHash(input);
			// Reverse the bytes since BigInteger uses little endian
			Array.Reverse(hash);
        
			BigInteger b = new BigInteger(hash);
			if (b < 0)
			{
				return "-" + (-b).ToString("x").TrimStart('0');
			}
			else
			{
				return b.ToString("x").TrimStart('0');
			}
			/*
			var sha1 = SHA1.Create();
			byte[] hash = sha1.ComputeHash(input);
			bool negative = (hash[0] & 0x80) == 0x80;
			if (negative) // check for negative hashes
				hash = TwosCompliment(hash);
			// Create the string and trim away the zeroes
			string digest = GetHexString(hash).TrimStart('0');
			if (negative)
				digest = "-" + digest;
			return digest;*/
		}

		/*private static string GetHexString(byte[] p)
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
		}*/

		public override void Dispose()
		{
			//World?.Ticker?.UnregisterTicked(this);

			var missingSounds = _missingSounds.TakeAndClear();

			base.Dispose();

			foreach (var disposable in _disposables.ToArray())
			{
				disposable.Dispose();
			}
			
			_disposables.Clear();

			Client.Stop();

			Client.Dispose();
			
			//Options.VideoOptions.RenderDistance.
		}
	}
}
