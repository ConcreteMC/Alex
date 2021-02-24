using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Alex.API.Graphics;
using Alex.API.Gui;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Entities.Effects;
using Alex.Entities.Generic;
using Alex.Entities.Projectiles;
using Alex.Gamestates;
using Alex.Graphics.Models.Entity;
using Alex.Graphics.Models.Entity.Animations;
using Alex.Gui.Elements;
using Alex.Items;
using Alex.Net.Bedrock;
using Alex.Net.Bedrock.Packets;
using Alex.Net.Bedrock.Raknet;
using Alex.ResourcePackLib.Json;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Services;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Utils.Inventories;
using Alex.Worlds.Abstraction;
using fNbt;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using MiNET;
using MiNET.Entities;
using MiNET.Net;
using MiNET.UI;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using AnvilWorldProvider = Alex.Worlds.Singleplayer.AnvilWorldProvider;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using ChunkCoordinates = Alex.API.Utils.ChunkCoordinates;
using Entity = Alex.Entities.Entity;
using MathF = System.MathF;
using MessageType = Alex.API.Data.MessageType;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Worlds.Multiplayer.Bedrock
{
	public class BedrockClientPacketHandler : IMcpeClientMessageHandler
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClientPacketHandler));

		private BedrockClient Client { get; }
		private Alex AlexInstance { get; }
        private CancellationToken CancellationToken { get; }
        private ChunkProcessor ChunkProcessor { get; }

        private WorldProvider  WorldProvider         { get; }
        private PlayerProfile  PlayerProfile         { get; }
        private IStorageSystem Storage               { get; }
        private bool           UseCustomEntityModels { get; set; }

        public BedrockClientPacketHandler(BedrockClient client, WorldProvider worldProvider, PlayerProfile profile, Alex alex, CancellationToken cancellationToken, ChunkProcessor chunkProcessor) //:
	       // base(client)
        {
	        //Plugin = plugin;
	        Storage = alex.Services.GetRequiredService<IStorageSystem>();

	        Client = client;
	        AlexInstance = alex;
	        CancellationToken = cancellationToken;
	        WorldProvider = worldProvider;
	        PlayerProfile = profile;
	        
	        AnvilWorldProvider.LoadBlockConverter();

	        ChunkProcessor = chunkProcessor;

	        var options = alex.Services.GetRequiredService<IOptionsProvider>().AlexOptions;
	        options.VideoOptions.CustomSkins.Bind((value, newValue) => UseCustomEntityModels = newValue);
	        UseCustomEntityModels = options.VideoOptions.CustomSkins.Value;
	        
	        
        }

        public  bool                  ReportUnhandled { get; set; } = true;
        private Dictionary<int, (Type type, ulong count)> UnhandledPackets = new Dictionary<int, (Type type, ulong count)>();
        private void UnhandledPackage(Packet packet)
		{
			if (UnhandledPackets.TryAdd(packet.Id, (packet.GetType(), 1)))
			{
				Log.Warn($"Unhandled bedrock packet: {packet.GetType().Name} (0x{packet.Id:X2})");
			}
			else
			{
				var value = UnhandledPackets[packet.Id];
				value.count++;
				UnhandledPackets[packet.Id] = value;
			}
		}

        public void HandleMcpeServerToClientHandshake(McpeServerToClientHandshake message)
        {
	        string token = message.token;

	        IDictionary<string, dynamic> headers = JWT.Headers(token);
	        string x5u = headers["x5u"].TrimEnd('=');

	        try
	        {
		       // Client.Connection.Session.FirstEncryptedMessage = message.ReliabilityHeader.OrderingIndex;
		        
		        var data = JWT.Payload<HandshakeData>(token);
		        Client.InitiateEncryption(Base64Url.Decode(x5u), Base64Url.Decode(data.salt.TrimEnd('=')));
	        }
	        catch (Exception e)
	        {
		        //AlexInstance.GameStateManager.Back();
		        string msg = $"Network error.";

		        if (e is Jose.IntegrityException)
		        {
			        msg = $"Invalid server signature!";
		        }
		        
		        Client.ShowDisconnect(msg, false, false, DisconnectReason.Network);
		        
		        Log.Error(e, $"Could not complete handshake: {e.ToString()}");
		        throw;
	        }
        }
        
        private bool _markedAsInitalized = false;
        public void HandleMcpePlayStatus(McpePlayStatus message)
		{
			Log.Info($"Client status: {message.status}");
			Client.PlayerStatus = message.status;

			if (Client.PlayerStatus == 3)
			{
				Client.MarkAsInitialized();
			}
			else if (Client.PlayerStatus == 0)
			{
				Log.Info($"Received Play Status packet: Login success");
				
				McpeClientCacheStatus status = McpeClientCacheStatus.CreateObject();
				status.enabled = ChunkProcessor.Cache.Enabled;
				Client.SendPacket(status);
			}
			else
			{
				Log.Warn($"Received unknown Play Status... {message.status}");
				Client.ShowDisconnect($"Unrecognized play status.", false, true, DisconnectReason.Network);
			}
		}

        public void HandleMcpeDisconnect(McpeDisconnect message)
        {
            Log.Info($"Received disconnect: {message.message}");
            Client.ShowDisconnect(message.message, false, true, DisconnectReason.Kicked);
            
           // Client.
           // base.HandleMcpeDisconnect(message);
        }
        
        public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
        {
	        Client.ResourcePackManager.HandleMcpeResourcePackDataInfo(message);
        }

        public void HandleMcpeResourcePackChunkData(McpeResourcePackChunkData message)
        {
	        Client.ResourcePackManager.HandleMcpeResourcePackChunkData(message);
        }

        private ResourcePackIds _resourcePackIds;
        public void HandleMcpeResourcePacksInfo(McpeResourcePacksInfo message)
        {
	        Client.ResourcePackManager.HandleMcpeResourcePacksInfo(message);
        }

        public void HandleMcpeResourcePackStack(McpeResourcePackStack message)
        {
	        Client.ResourcePackManager.HandleMcpeResourcePackStack(message);
        }

        public void HandleMcpeText(McpeText message)
        {
	        string rawMessage = message.message;
			switch (message.type)
			{
				case 0: //Raw
					break;
				case 2: //Translation
					break;
				
				case 1: //Chat
					if (!string.IsNullOrWhiteSpace(message.source))
					{
						rawMessage = $"<{message.source}{ChatFormatting.Reset}>: {rawMessage}";
					}

					break;
				case 7: //Whisper
					rawMessage = $"<{message.source}{ChatFormatting.Reset} whispered>: {rawMessage}";
					break;
				case 8: //Announcement
					rawMessage = $"[{message.source}{ChatFormatting.Reset}]: {rawMessage}";
					break;
			}
			WorldProvider?.ChatRecipient?.AddMessage(rawMessage, (MessageType) message.type);
		//	EventDispatcher.DispatchEvent(new ChatMessageReceivedEvent(new ChatObject(message.message), (MessageType) message.type));
		}

		public void HandleMcpeSetTime(McpeSetTime message)
		{
			Client.World?.SetTime(message.time, message.time % 24000);
			
			Client.ChangeDimensionResetEvent.Set();
		}
		
		public void HandleMcpeStartGame(McpeStartGame message)
		{
			Log.Info($"Start game.");
			
			//McpeClientCacheStatus status = McpeClientCacheStatus.CreateObject();
		//	status.enabled = ChunkProcessor.Cache.Enabled;
			//Client.SendPacket(status);
			
			try
			{
				Client.World.Player.EntityId = Client.EntityId = message.runtimeEntityId;
				Client.NetworkEntityId = message.entityIdSelf;

				Client.SpawnPoint = new Vector3(
					message.spawn.X, message.spawn.Y - Player.EyeLevel, message.spawn.Z); //message.spawn;

				//Client.CurrentLocation = new MiNET.Utils.PlayerLocation(Client.SpawnPoint, message.spawn.X, message.spawn.X, message.spawn.Y);
				Client.World.Dimension = (Dimension) message.dimension;

				Client.World?.UpdatePlayerPosition(
					new API.Utils.PlayerLocation(
						new Microsoft.Xna.Framework.Vector3(
							Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z), message.spawn.X,
						message.spawn.X, message.spawn.Y));

				if (message.enableNewInventorySystem)
				{
					Log.Info($"Using new transaction based inventory.");
					Client.World.Player.SetInventory(new ItemStackInventory(Client));
				}
				else
				{
					Client.World.Player.SetInventory(new BedrockInventory(46));
				}

				Client.World.Player.UpdateGamemode((GameMode) message.playerGamemode);

				foreach (var gr in message.gamerules)
				{
					Client.World.SetGameRule(gr);
				}
				
				//message.itemstates[0].

				ChunkProcessor.Itemstates = message.itemstates;

				Dictionary<uint, BlockStateContainer> ourStates = new Dictionary<uint, BlockStateContainer>();

				foreach (var bs in message.blockPalette)
				{
					foreach (var blockstate in bs.States)
					{
						var name = blockstate.Name;

						if (name != null)
						{
							if (name.Equals("minecraft:grass", StringComparison.InvariantCultureIgnoreCase))
								name = "minecraft:grass_block";

							blockstate.Name = name;
						}
					}

					var name2 = bs.Name;

					if (name2 != null)
					{
						if (name2.Equals("minecraft:grass", StringComparison.InvariantCultureIgnoreCase))
							name2 = "minecraft:grass_block";

						bs.Name = name2;
					}

					ourStates.TryAdd((uint) bs.RuntimeId, bs);
				}

				ChunkProcessor.BlockStateMap = ourStates;
			}
			finally
			{
				Client.GameStarted = true;
				
				Client.RequestChunkRadius(Client.ChunkRadius);
			}

			//	_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.World.UpdateEntityPosition(message.runtimeEntityId, 
					new PlayerLocation(message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch), adjustForEntityHeight:true);
				return;
			}
			
			Client.World.UpdatePlayerPosition(new 
				PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch), message.mode == 2);

			Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.x, message.y, message.z), Client.World.Player.KnownPosition.OnGround);
		}


		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			//Client.UserPermission = (CommandPermission) message.commandPermission;
			Client.World.Player.CanFly = ((message.flags & 0x40) == 0x40);
			Client.World.Player.IsFlying = ((message.flags & 0x200) == 0x200);
			Client.World.Player.IsWorldImmutable = ((message.flags & 0x01) == 0x01);
			Client.World.Player.IsNoPvP = (message.flags & 0x02) == 0x02;
			Client.World.Player.IsNoPvM = (message.flags & 0x04) == 0x04;
			Client.World.Player.HasCollision = (message.flags & 0x80) != 0x80;
		}

		public void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
			if (!Client.World.EntityManager.TryGet(message.uuid, out var entity))
			{
				RemotePlayer player = new RemotePlayer(message.username, Client.World, Client);
				player.EntityId = message.runtimeEntityId;
				player.UUID = message.uuid;

				entity = player;
				//	Client.World?.AddPlayerListItem(new PlayerListItem(message.uuid, message.username, 0, 0, false));
				//	mob = new RemotePlayer():


				//mob.EntityId = message.runtimeEntityId;


			}

			if (entity == null)
			{
				var identifier = "";

				if (!string.IsNullOrWhiteSpace(message.username))
				{
					identifier = message.username.Replace("\n", "");
				}

				Log.Warn(
					$"({message.ReliabilityHeader.ReliableMessageNumber} | {message.ReliabilityHeader.OrderingIndex} | {message.ReliabilityHeader.SequencingIndex}) Tried spawning invalid player: {identifier} (UUID: {message.uuid} EntityID: {message.runtimeEntityId}))");
				
				return;
			}

			entity.KnownPosition = new PlayerLocation(
				message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch) {OnGround = true};

			entity.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);

			if (message.item != null)
			{
				entity.Inventory.MainHand = message.item.ToAlexItem();
			}

			if (message.metadata != null)
			{
				entity.HandleMetadata(message.metadata);
			}

			
		//	entity.KnownPosition.Y -= Pl (float) (entity.BoundingBox.GetHeight() - 0.175f);
				//mob.HandleMetadata();
			//	message.flags

			//	mob.IsSpawned = true;

			//	_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
			Client.World.SpawnEntity(entity);
		}

		public void HandleMcpePlayerList(McpePlayerList message)
		{
			if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					Client.World.AddPlayerListItem(new PlayerListItem(r.ClientUuid, r.DisplayName, (GameMode)((int)r.GameMode), 0, false));

					bool         isNewEntity = true;
					RemotePlayer m           = null;
					if (Client.World.EntityManager.TryGet(r.ClientUuid, out var entity) && entity is RemotePlayer rp)
					{
						m = rp;
						isNewEntity = false;
						//Client.World.EntityManager.UpdateEntityId(entity, r.EntityId);
					}
					else
					{
						m = new RemotePlayer(r.DisplayName, Client.World, Client);
						m.EntityId = r.EntityId;
					}
					
					m.UUID = r.ClientUuid;
					m.Skin = r.Skin;

					if (isNewEntity)
					{
						m.SetInventory(new BedrockInventory(46));
						Client.World.EntityManager.AddEntity(m);
					}
				}
			}
            else if (message.records is PlayerRemoveRecords removeRecords)
            {
	            foreach (var r in removeRecords)
	            {
		            Client.World.RemovePlayerListItem(r.ClientUuid);
	            }
            }
		}

		public bool SpawnMob(long entityId,
			EntityType type,
			PlayerLocation position,
			Microsoft.Xna.Framework.Vector3 velocity, EntityAttributes attributes, MetadataDictionary metadata)
		{
			
			Entity entity = null;

			if (type == EntityType.FallingBlock)
			{
				entity = new EntityFallingBlock(null);
			}
			else
			{
				EntityModelRenderer renderer = EntityFactory.GetEntityRenderer(
					$"minecraft:{type.ToString().ToLower()}", null);

				if (renderer != null)
				{
					entity = EntityFactory.Create(type, null);
				}

				if (entity == null)
				{
					entity = new Entity(null, Client);
				}

				if (renderer == null)
				{
					Log.Debug($"Missing renderer for entity: {type.ToString()} ({(int) type})");

					return false;
				}

				if (renderer.Texture == null)
				{
					Log.Debug($"Missing texture for entity: {type.ToString()} ({(int) type})");

					return false;
				}

				entity.ModelRenderer = renderer;
			}

			var uuid = MiNETExtensions.FromEntityId(entityId);
			entity.KnownPosition = position;
			entity.Velocity = velocity;
			entity.EntityId = entityId;
			entity.UUID = uuid;
			entity.SetInventory(new BedrockInventory(46));
			
			if (metadata != null)
				entity.HandleMetadata(metadata);
			//entity.ad
			
			Client.World.SpawnEntity(entity);

			return true;
		}


		public void HandleMcpeAddEntity(McpeAddEntity message)
		{
			MiNET.Entities.EntityType entityType;
			if (_entityIdentifiers.TryGetValue(message.entityType, out var realId))
			{
				entityType = (MiNET.Entities.EntityType) realId;
			}
			else
			{
				entityType = MiNET.Entities.EntityHelpers.ToEntityType(message.entityType);
			}

			SpawnMob(message.runtimeEntityId,entityType,
				new PlayerLocation(message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch),
				new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ),
				message.attributes, message.metadata);
        }

		//private ConcurrentDictionary<long, long> _entityMapping = new ConcurrentDictionary<long, long>();
		public void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			//if (_entityMapping.TryRemove(message.entityIdSelf, out var entityId))
			{
				Client.World.DespawnEntity(message.entityIdSelf);
			}
		}

		public void HandleMcpeAddItemEntity(McpeAddItemEntity message)
		{
			var slot = message.item;
			var item = message.item.ToAlexItem();

			var itemClone = item.Clone();

			itemClone.Count = slot.Count;
			itemClone.Nbt = slot.ExtraData;

			ItemEntity itemEntity = new ItemEntity(null);
			itemEntity.EntityId = message.runtimeEntityId;
			itemEntity.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);
			itemEntity.KnownPosition = new PlayerLocation(message.x, message.y, message.z);
			itemEntity.HandleMetadata(message.metadata);

			itemEntity.SetItem(itemClone);

			if (Client.World.SpawnEntity(itemEntity))
			{
			//	_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
			}
			else
			{
				Log.Warn(
					$"Could not spawn in item entity, an entity with this runtimeEntityId already exists! (Runtime: {message.runtimeEntityId} Self: {message.entityIdSelf})");
			}
		}

		public void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
		{
			if (Client.World.EntityManager.TryGet(message.target, out var targetEntity)
			    && Client.World.EntityManager.TryGet(message.runtimeEntityId, out var runtimeEntity))
			{
				ItemEntity itemEntity;
				Entity target;

				if (runtimeEntity is ItemEntity rte)
				{
					itemEntity = rte;
					target = targetEntity;
				}
				else if (targetEntity is ItemEntity rte2)
				{
					itemEntity = rte2;
					target = runtimeEntity;
				}
				else
				{
					Log.Warn($"Invalid TakeItemEntity request.");
					return;
				}

				var targetBoundingBox = target.GetBoundingBox(target.KnownPosition);

				var direction = (itemEntity.KnownPosition.ToVector3() - new Microsoft.Xna.Framework.Vector3(
					target.KnownPosition.X, targetBoundingBox.Max.Y, target.KnownPosition.Z));

				itemEntity.Velocity = direction / 20f;
			}
		}

		public void HandleMcpeMoveEntity(McpeMoveEntity message)
		{
			var location = new PlayerLocation(
				message.position.X, message.position.Y, message.position.Z, -message.position.HeadYaw,
				-message.position.Yaw, -message.position.Pitch);

			if (message.runtimeEntityId != Client.EntityId)
			{
				Client.World.UpdateEntityPosition(message.runtimeEntityId, location, false, true, true, false, true);

				return;
			}

			location.Y -= Player.EyeLevel;
			Client.World.UpdatePlayerPosition(location);
		}

		public void HandleMcpeMoveEntityDelta(McpeMoveEntityDelta message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				if (message is EntityDelta ed)
				{
					if (Client.World.TryGetEntity(message.runtimeEntityId, out var entity))
					{
						var known = ed.GetCurrentPosition(entity.KnownPosition);
						entity.Movement.MoveTo(known);
					}
				}
			}
		}

		public void HandleMcpeRiderJump(McpeRiderJump message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlock(McpeUpdateBlock message)
		{
			var converted = ChunkProcessor.GetBlockState(message.blockRuntimeId);

			if (converted != null)
			{
				BlockUpdatePriority priority = (BlockUpdatePriority) message.blockPriority;

				if ((priority & BlockUpdatePriority.NoGraphic) != 0)
				{
					
				}
				
				Client.World?.SetBlockState(
					new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z), 
					converted, (int) message.storage, priority);
			}
			else
			{
				Log.Warn($"Received unknown block runtime id.");
			}
		}

		public void HandleMcpeAddPainting(McpeAddPainting message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTickSync(McpeTickSync message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLevelSoundEventOld(McpeLevelSoundEventOld message)
		{
			UnhandledPackage(message);
        }

		public void HandleMcpeSpawnParticleEffect(McpeSpawnParticleEffect message)
		{
			UnhandledPackage(message);
        }

		private ConcurrentDictionary<string, int> _entityIdentifiers = new ConcurrentDictionary<string, int>();
		public void HandleMcpeAvailableEntityIdentifiers(McpeAvailableEntityIdentifiers message)
		{
			var idList = message.namedtag.NbtFile?.RootTag?["idlist"];

			if (idList != null && idList is NbtList ids)
			{
				foreach (NbtCompound tag in ids.ToArray<NbtCompound>())
				{
					if (tag.TryGet("id", out NbtString id)
					&& tag.TryGet("rid", out NbtInt realId))
					{
						_entityIdentifiers[id.Value] = realId.Value;
					}
				}
			}
			//UnhandledPackage(message);
        }

		public void HandleMcpeLevelSoundEventV2(McpeLevelSoundEventV2 message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNetworkSettings(McpeNetworkSettings message)
		{
			//Client.Connection.Session.
			var threshold = BitConverter.ToInt16(new byte[] {message.unknown, message.compressionThreshold});
			Client.Connection.Session.CompressionThreshold = threshold;
			
			Log.Info($"Compression Threshold: {threshold}");
			//UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeCreativeContent(McpeCreativeContent message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpePlayerEnchantOptions(McpePlayerEnchantOptions message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeItemStackResponse(McpeItemStackResponse message)
		{
			//UnhandledPackage(message);
			if (Client.World.Player.Inventory is ItemStackInventory itemStackInventory)
			{
				itemStackInventory.HandleResponses(message.responses);
			}
		}

		/// <inheritdoc />
		public void HandleMcpeItemComponent(McpeItemComponent message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeFilterTextPacket(McpeFilterTextPacket message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeAlexEntityAnimation(McpeAlexEntityAnimation message)
		{
			Entity entity;
			if (!Client.World.EntityManager.TryGet(message.runtimeEntityId, out entity))
			{
				if (message.runtimeEntityId != Client.World.Player.EntityId)
				{
					Log.Warn($"Got animation request for unknown entity: {entity}");

					return;
				}
				
				entity = Client.World.Player;
			}

			if (!entity.ModelRenderer.GetBone(message.boneId, out var bone))
			{
				Log.Warn($"Animation issue: Could not find bone with name {message.boneId}");

				return;
			}

			if (message.keys.Length == 0)
			{
				Log.Warn($"Invalid animation: 0 keys.");

				return;
			}

			if (message.keys.First().ExecuteImmediate)
			{
				bone.ClearAnimations();
			}

			foreach (var key in message.keys)
			{
				bone.Animations.Enqueue(
					new ServerAnimation(
						bone,
						new ModelParameters(
							new Microsoft.Xna.Framework.Vector3(
								key.StartRotation.X, key.StartRotation.Y, key.StartRotation.Z)),
						new ModelParameters(
							new Microsoft.Xna.Framework.Vector3(
								key.EndRotation.X, key.EndRotation.Y, key.EndRotation.Z)), TimeSpan.FromMilliseconds(key.Duration), key.ResetAfter));
			}

			bone.Animations.Enqueue(new ResetAnimation(bone));
		}

		public void HandleMcpeNetworkChunkPublisherUpdate(McpeNetworkChunkPublisherUpdate message)
		{
			Client.LastChunkPublish = message;
			//UnhandledPackage(message);
			//Log.Info($"Chunk publisher update: {message.coordinates} | {message.radius}");
		}

		public void HandleMcpeBiomeDefinitionList(McpeBiomeDefinitionList message)
		{
			UnhandledPackage(message);
        }

		public void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLevelEventGeneric(McpeLevelEventGeneric message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeLecternUpdate(McpeLecternUpdate message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeVideoStreamConnect(McpeVideoStreamConnect message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientCacheStatus(MiNET.Net.McpeClientCacheStatus message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeOnScreenTextureAnimation(McpeOnScreenTextureAnimation message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMapCreateLockedCopy(McpeMapCreateLockedCopy message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStructureTemplateDataExportRequest(McpeStructureTemplateDataExportRequest message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStructureTemplateDataExportResponse(McpeStructureTemplateDataExportResponse message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlockProperties(McpeUpdateBlockProperties message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientCacheBlobStatus(McpeClientCacheBlobStatus message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientCacheMissResponse(McpeClientCacheMissResponse message)
		{
			//UnhandledPackage(message);
			
			ChunkProcessor.HandleClientCacheMissResponse(message);
		}

		public void HandleMcpeLevelEvent(McpeLevelEvent message)
		{
			if (!Client.World.Player.IsBreakingBlock)
				return;

			if ((BlockCoordinates) new Microsoft.Xna.Framework.Vector3(
				message.position.X, message.position.Y, message.position.Z) == Client.World.Player.TargetBlock)
			{
				if (message.eventId == 3600)
				{
					var ticksRequired = (double) ushort.MaxValue / message.data;
					Client.World.Player.BreakTimeNeeded = ticksRequired;
				}
			}
		}

		public void HandleMcpeBlockEvent(McpeBlockEvent message)
		{
			var worldPos = new BlockCoordinates(
				message.coordinates.X, message.coordinates.Y, message.coordinates.Z);

			if (Client.World.EntityManager.TryGetBlockEntity(worldPos, out var blockEntity))
			{
				blockEntity.HandleBlockAction((byte) message.case1, message.case2);	
			}
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			if (Client.World.TryGetEntity(message.runtimeEntityId, out Entity entity))
			{
				if (message.eventId == 2)
				{
					entity.EntityHurt();
				}
				else if (message.eventId == 4)
				{
					entity.SwingArm();
				}
			}
			//UnhandledPackage(message);
		}

		public void HandleMcpeMobEffect(McpeMobEffect message)
		{
			Entity entity;

			if (!Client.World.EntityManager.TryGet(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
					entity = Client.World.Player;
			}

			if (entity == null)
			{
				//Log.Warn($"Could not find entity targeted in McpeMobEffect.");
				return;
			}

			Effect     effect     = null;
			EffectType effectType = (EffectType) message.effectId;
			switch (effectType)
			{
				case EffectType.Speed:
					effect = new SpeedEffect();
					break;

				case EffectType.JumpBoost:
					effect = new JumpBoostEffect();
					break;
				
				case EffectType.NightVision:
					effect = new NightVisionEffect();
					break;
				
				default:
					Log.Warn($"Missing effect implementation: {(EffectType) message.effectId}");
					return;
			}

			switch (message.eventId)
			{
				case 1: //Add
					effect.Duration = message.duration;
					effect.Level = message.amplifier;
					effect.Particles = message.particles;
					entity.AddOrUpdateEffect(effect);
					break;
				
				case 2: //Modify
					if (entity.TryGetEffect(effectType, out effect))
					{
						effect.Duration = message.duration;
						effect.Particles = message.particles;
						effect.Level = message.amplifier;
						
						entity.AddOrUpdateEffect(effect);
					}
					break;

				case 3: //Remove
					entity.RemoveEffect(effect.EffectId);
					break;
			}
			//UnhandledPackage(message);

		}

		public void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
		{
			var player = Client.World.Player;
			if (player.EntityId == message.runtimeEntityId)
			{
				if (message.attributes.TryGetValue("minecraft:health", out var value))
				{
					player.HealthManager.MaxHealth = (int) value.MaxValue;
					player.HealthManager.Health = (int) value.Value;
				}

				if (message.attributes.TryGetValue("minecraft:movement", out var movement))
				{
					player.MovementSpeed = movement.Value;
				}

				if (message.attributes.TryGetValue("minecraft:player.hunger", out var hunger))
				{
					player.HealthManager.Hunger = (int) hunger.Value;
					player.HealthManager.MaxHunger = (int) hunger.MaxValue;
				}
				
				if (message.attributes.TryGetValue("minecraft:player.exhaustion", out var exhaustion))
				{
					player.HealthManager.Exhaustion = (int) exhaustion.Value;
					player.HealthManager.MaxExhaustion = (int) exhaustion.MaxValue;
				}
				
				if (message.attributes.TryGetValue("minecraft:player.saturation", out var saturation))
				{
					player.HealthManager.Saturation = (int) saturation.Value;
					player.HealthManager.MaxSaturation = (int) saturation.MaxValue;
				}
			}
		}

		public void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			if (Client.World.Player.EntityId == message.runtimeEntityId)
			{
				Client.World.Player.Inventory.SelectedSlot = message.selectedSlot;
				return;
			}
			
			if (Client.World.TryGetEntity(message.runtimeEntityId, out var entity))
			{
				var item = message.item.ToAlexItem().Clone();

				byte slot = message.slot;
				switch (message.windowsId)
				{
					case 0:
						
						break;
				}
				
				entity.Inventory[slot] = item;
				entity.Inventory.SelectedSlot = message.selectedSlot;
			}

		//	UnhandledPackage(message);
		}

		public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			Entity entity = null;

			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
				}

				//entity.Inventory.Boots
			}

			if (entity == null)
			{
				Log.Warn($"Unknown entity in MobArmorEquipment packet! ({message.runtimeEntityId})");

				return;
			}


			entity.Inventory.Helmet = message.helmet.ToAlexItem().Clone();
			entity.Inventory.Chestplate = message.chestplate.ToAlexItem().Clone();
			entity.Inventory.Leggings = message.leggings.ToAlexItem().Clone();
			entity.Inventory.Boots = message.boots.ToAlexItem().Clone();


			//UnhandledPackage(message);
		}

		public void HandleMcpeInteract(McpeInteract message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeHurtArmor(McpeHurtArmor message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityData(McpeSetEntityData message)
		{
			Entity entity = null;
			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
				}
			}

			if (entity == null)
				return;

			/*if (entity is EntityFallingBlock fallingBlock)
			{
				foreach (var meta in message.metadata._entries.ToArray())
				{
					switch ((MiNET.Entities.Entity.MetadataFlags) meta.Key)
					{
						case MiNET.Entities.Entity.MetadataFlags.Variant:
							message.metadata._entries.Remove(meta.Key);

							if (meta.Value is MetadataInt metaInt)
							{
								var blockState = ChunkProcessor.GetBlockState((uint) metaInt.Value);

								if (ItemFactory.TryGetItem(blockState.Name, out var item))
								{
									fallingBlock.SetItem(item);
								}
								else
								{
									Log.Info($"Could not get item: {blockState.Name}");
								}
							}

							break;
					}
				}
			}*/

			entity.HandleMetadata(message.metadata);
			//UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			var v = message.velocity;
			var velocity = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);

			Entity entity = null;
			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.World.Player.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
				}
			}

			if (entity == null)
				return;
			
			entity.Movement.Velocity(velocity);
			//var old = entity.Velocity;
			//entity.Velocity += new Microsoft.Xna.Framework.Vector3(velocity.X - old.X, velocity.Y - old.Y, velocity.Z - old.Z);

			//UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityLink(McpeSetEntityLink message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetHealth(McpeSetHealth message)
		{
			Client.World.Player.HealthManager.Health = message.health;
		}

		public void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
		{
			Client.SpawnPoint = new Vector3(
				message.coordinates.X, (float) (message.coordinates.Y), message.coordinates.Z);

			Client.LevelInfo.SpawnX = (int) Client.SpawnPoint.X;
			Client.LevelInfo.SpawnY = (int) Client.SpawnPoint.Y;
			Client.LevelInfo.SpawnZ = (int) Client.SpawnPoint.Z;

			Client.World.SpawnPoint = new Microsoft.Xna.Framework.Vector3(
				Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z);
		}

		public void HandleMcpeAnimate(McpeAnimate message)
		{
			if (Client.World.EntityManager.TryGet(message.runtimeEntityId, out Entity entity))
			{
				switch (message.actionId)
				{
					case 1:
						entity.SwingArm();
						break;
					case 4: //Critical hit!
						entity.EntityHurt();
						break;
					default:
						Log.Info($"Unknown McpeAnimate action ID: {message.actionId}");
						break;
				}
			}
		}

		public void HandleMcpeRespawn(McpeRespawn message)
		{
			Log.Info($"Respawn state {message.state} | Runtime entity id: {message.runtimeEntityId}");

			if (message.state == 0)
			{
				var response = McpeRespawn.CreateObject();
				response.runtimeEntityId = message.runtimeEntityId;
				response.x = message.x;
				response.y = message.y;
				response.z = message.z;
				response.state = 2;
				
				Client.SendPacket(response);
			}
			else if (message.state == 1)
			{
				Client.World.UpdatePlayerPosition(new PlayerLocation(message.x, message.y, message.z));

				if (Client.CanSpawn)
				{
					Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(message.x, message.y, message.z), false);
				}

				Client.RequestChunkRadius(Client.ChunkRadius);

				Client.ChangeDimensionResetEvent.Set();
			}
		}

		private void InventoryOnCursorChanged(object sender, CursorChangedEventArgs e)
		{
			if (e.IsServerTransaction)
				return;

			
		}
		
		private void InventoryOnSlotChanged(object sender, SlotChangedEventArgs e)
		{
			if (e.IsServerTransaction)
				return;

			TakeAction a = new TakeAction();
			
		}
		
		public void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			try
			{
				var windowId = message.windowId;
				var dialog = Client.World.InventoryManager.Show(Client.World.Player.Inventory, message.windowId, (ContainerType) message.type);
				dialog.Inventory.CursorChanged += InventoryOnCursorChanged;
				dialog.Inventory.SlotChanged += InventoryOnSlotChanged;
				
				dialog.OnContainerClose += (sender, args) =>
				{
					dialog.Inventory.CursorChanged -= InventoryOnCursorChanged;
					dialog.Inventory.SlotChanged -= InventoryOnSlotChanged;
					
					var packet = McpeContainerClose.CreateObject();
					packet.windowId = windowId;
					Client.SendPacket(packet);
				};
			}
			catch (Exception)
			{
				Log.Warn($"Got unsupported container type: {message.type}");
			}
		}

		public void HandleMcpeContainerClose(McpeContainerClose message)
		{
			Client.World.InventoryManager.Close(message.windowId);
		}

        public void HandleMcpeInventoryContent(McpeInventoryContent message)
		{
			if (message.input == null)
			{
				Log.Warn($"Invalid inventory content! Input was null.");
				return;
			}

			var startIndex = 0;
			if (Client.World.InventoryManager.TryGet((int) message.inventoryId, out var container))
			{
				for (var index = 0; index < message.input.Count; index++)
				{
					var slot = message.input[index];

					var result = slot.ToAlexItem().Clone();
					if (result != null)
					{
						result.StackID = slot.UniqueId;
						container.Inventory.SetSlot(startIndex+ index, result, true);
						//inventory[usedIndex] = result;
					}
					else
					{
						Log.Warn($"Failed to set window slot: {index} Id: {slot.Id}:{slot.Metadata} (Window={message.inventoryId})");
					}
				}

				return;
			}
			
			if (Client.World.Player.Inventory is ItemStackInventory isi)
			{
				isi.HandleInventoryContent(message.inventoryId, message.input);
				return;
			}
			
			InventoryBase inventory  = null;
			//var 
			if (message.inventoryId == 0x00 //Inventory
			 //   || message.inventoryId == 124 //UI
			    || message.inventoryId == 120 //Armor
			    || message.inventoryId == 119 //Offhand
			 ) 
			{
				
				inventory = Client.World.Player.Inventory;
				if (inventory is BedrockInventory bi)
				{
					if (message.inventoryId == 0)
					{
						//startIndex = bi.InventoryOffset;
					}
					else if (message.inventoryId == 120)
					{
						startIndex = bi.BootsSlot;
					} 
					else if (message.inventoryId == 119)
					{
						startIndex = bi.OffHandSlot;
					}

				/*	if (message.inventoryId == 124)
					{
						startIndex = bi.slot
					}*/
				}
			}

			if (inventory == null)
			{
				Log.Warn($"(Inventory content) Unknown inventory ID: {message.inventoryId}");
				return;
			}

			for (var index = 0; index < message.input.Count; index++)
			{
				var slot = message.input[index];
				
				var usedIndex = index;

				var result = slot.ToAlexItem().Clone();
				if (result != null)
				{
					result.StackID = slot.UniqueId;
					inventory.SetSlot(startIndex + index, result, true);
					//inventory[usedIndex] = result;
				}
				else
                {
                    Log.Warn($"Failed to set slot: {index} Id: {slot.Id}:{slot.Metadata}");
                }
            }
		}

        public void HandleMcpeInventorySlot(McpeInventorySlot message)
		{
			if (message.item == null)
			{
				Log.Warn($"Invalid inventory slot packet! Item was null.");
				return;
			}

			if (Client.World.InventoryManager.TryGet((int) message.inventoryId, out var container))
			{
				var slotIndex  = (int)message.slot;
				var item = message.item.ToAlexItem().Clone();

				if (item != null)
				{
					container.Inventory.SetSlot(slotIndex, item, true);
				}
				return;
			}

			if (Client.World.Player.Inventory is ItemStackInventory isi)
			{
				isi.HandleSetSlot(message);

				return;
			}
			
			InventoryBase inventory = null;

			if (message.inventoryId == 0x00 //Inventory
			    || message.inventoryId == 124 //UI
			    || message.inventoryId == 120 //Armor
			    || message.inventoryId == 119 //Offhand
                                     )
			{
				inventory = Client.World.Player.Inventory;
			}

			if (inventory == null)
			{
				Log.Warn($"(McpeInventorySlot) Unknown inventory ID: {message.inventoryId} ({JsonConvert.SerializeObject(message.item)})");
				return;
			}
			
			var index = (int)message.slot;
			var result = message.item.ToAlexItem().Clone();

			if (result != null)
            {
	           // result.StackID = message.uniqueid;
	           switch (message.inventoryId)
	           {
		           case 119:
		           {
			           if (inventory is BedrockInventory bi)
			           {
				           bi.OffHand = result;
			           }

			           break;
		           }

		           case 120:
		           {
			           if (inventory is BedrockInventory bi)
			           {
				           bi.SetSlot(bi.BootsSlot + index, result, true);
			           }

			           // inventory.SetSlot();
			           break;
		           }

		           case 124:
			           inventory.SetCursor(result, true);
			           break;
		           default:
			           inventory.SetSlot(index, result, true);
			          // inventory[index] = result;
			           break;
	           }
            }
            else
            {
                Log.Warn($"Failed to set slot: {message.slot} Id: {message.item.Id}:{message.item.Metadata}");
            }
		}

        public void HandleMcpePlayerHotbar(McpePlayerHotbar message)
        {
            UnhandledPackage(message);
        }

        public void HandleMcpeContainerSetData(McpeContainerSetData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeCraftingData(McpeCraftingData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeCraftingEvent(McpeCraftingEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeGuiDataPickItem(McpeGuiDataPickItem message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeBlockEntityData(McpeBlockEntityData message)
		{
			if (message.namedtag.NbtFile.RootTag is NbtCompound compound)
			{
				var worldPos = new BlockCoordinates(
					message.coordinates.X, message.coordinates.Y, message.coordinates.Z);

				var block = Client.World.GetBlockState(worldPos).Block;

				//if (block.BlockMaterial != Material.Air)
				{
					var blockEntity = BlockEntityFactory.ReadFrom(compound, Client.World, block);

					if (blockEntity == null)
					{
						return;
					}

					Client.World.SetBlockEntity(
						message.coordinates.X, message.coordinates.Y, message.coordinates.Z, blockEntity);
				}
			}
			else
			{
				Log.Warn($"Invalid roottag for BlockEntityData. Got: {message.namedtag.NbtFile.RootTag.TagType}");
			}
		}

		public void HandleMcpeLevelChunk(McpeLevelChunk msg)
		{
			var blobs         = msg.blobHashes;
			var cacheEnabled  = msg.cacheEnabled;
			var subChunkCount = msg.subChunkCount;
			var chunkData     = msg.chunkData;
			var cx            = msg.chunkX;
			var cz            = msg.chunkZ;
			msg.PutPool();

			//if (chunkData[0] < 1)
			if (subChunkCount < 1)
			{
				//Nothing to read.
				return;
			}
			
			ChunkProcessor.HandleChunkData(cacheEnabled, blobs, subChunkCount, chunkData, cx, cz,
				column =>
				{ 
					Client.World.ChunkManager.AddChunk(column, new ChunkCoordinates(column.X, column.Z), true);
					
					//EventDispatcher.DispatchEvent(
					//	new ChunkReceivedEvent(new ChunkCoordinates(column.X, column.Z), column));
				});
		}

		public void HandleMcpeChangeDimension(McpeChangeDimension message)
		{
			//base.HandleMcpeChangeDimension(message);

			var chunkCoords = new ChunkCoordinates(
				new PlayerLocation(Client.World.SpawnPoint.X, Client.World.SpawnPoint.Y, Client.World.SpawnPoint.Z));

			Client.World.Player.IsSpawned = false;
			LoadingWorldScreen loadingWorldScreen = new LoadingWorldScreen() {ConnectingToServer = true};

			AlexInstance.GuiManager.AddScreen(loadingWorldScreen);
			//	AlexInstance.GameStateManager.SetActiveState(loadingWorldState, true);
			loadingWorldScreen.UpdateProgress(LoadingState.LoadingChunks, 0);

			ThreadPool.QueueUserWorkItem(
				(o) =>
				{
					World world = Client.World;

					world.ClearChunksAndEntities();


					//world.ChunkManager.ClearChunks();
					world.UpdatePlayerPosition(
						new PlayerLocation(message.position.X, message.position.Y, message.position.Z));


					//foreach (var loadedChunk in provider.LoadedChunks)
					//{
					//	provider.UnloadChunk(loadedChunk);
					//}

					int  percentage         = 0;
					bool ready              = false;
					int  previousPercentage = 0;

					do
					{
						chunkCoords = new ChunkCoordinates(
							new PlayerLocation(
								Client.World.Player.KnownPosition.X, Client.World.Player.KnownPosition.Y,
								Client.World.Player.KnownPosition.Z));


						double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
						var    target        = radiusSquared;

						percentage = (int) ((100 / target) * world.ChunkManager.ChunkCount);

						if (percentage != previousPercentage)
						{
							loadingWorldScreen.UpdateProgress(LoadingState.LoadingChunks, percentage);
							previousPercentage = percentage;

							//Log.Info($"Progress: {percentage} ({ChunksReceived} of {target})");
						}

						//if (!ready)
						//{
						if (!ready)
						{
							if (Client.ChangeDimensionResetEvent.WaitOne(5))
							{
								ready = true;
							}
						}


						if (percentage >= 100)
						{
							break;
						}

						//	}
						//	else
						//	{
						//	await Task.Delay(50);
						//}
					} while (true);

					McpePlayerAction action = McpePlayerAction.CreateObject();
					action.runtimeEntityId = Client.EntityId;
					action.actionId = (int) PlayerAction.DimensionChangeAck;
					Client.SendPacket(action);

					AlexInstance.GuiManager.RemoveScreen(loadingWorldScreen);
					//AlexInstance.GameStateManager.Back();

					var p = Client.World.Player.KnownPosition;

					Client.SendMcpeMovePlayer(
						new MiNET.Utils.PlayerLocation(p.X, p.Y, p.Z, p.HeadYaw, p.Yaw, p.Pitch),
						Client.World.Player.KnownPosition.OnGround);
					//Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(Client.World.Player.KnownPosition.X, Client.World.Player.KnownPosition.Y, Client.World.Player.KnownPosition.Z), false);

					Client.World.Player.IsSpawned = true;
				});
		}

		public void HandleMcpeSetCommandsEnabled(McpeSetCommandsEnabled message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetDifficulty(McpeSetDifficulty message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
		{
			Client.World.Player.UpdateGamemode((GameMode) message.gamemode);
		}

		public void HandleMcpeSimpleEvent(McpeSimpleEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTelemetryEvent(McpeTelemetryEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSpawnExperienceOrb(McpeSpawnExperienceOrb message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeClientboundMapItemData(McpeClientboundMapItemData message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
		{
			Log.Info($"Illegal chunkradius request: {message.chunkRadius}");
		//	 Client.RequestChunkRadius(Client.ChunkRadius);
		}

		public void HandleMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate message)
		{
			Client.ChunkRadius = message.chunkRadius;
			//if (!Client.CanSpawn && Client.GameStarted)
			//	Client.MarkAsInitialized();
			//UnhandledPackage(message);
		}

		public void HandleMcpeItemFrameDropItem(McpeItemFrameDropItem message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeGameRulesChanged(McpeGameRulesChanged message)
		{
			var lvl = Client.World;
			
			foreach (var gr in message.rules)
			{
				lvl.SetGameRule(gr);
			}
		}

		public void HandleMcpeCamera(McpeCamera message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeBossEvent(McpeBossEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeShowCredits(McpeShowCredits message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeAvailableCommands(McpeAvailableCommands message)
		{
			// Client.LoadCommands(message.CommandSet);
			UnhandledPackage(message);
		}

		public void HandleMcpeCommandOutput(McpeCommandOutput message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateTrade(McpeUpdateTrade message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateEquipment(McpeUpdateEquipment message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTransfer(McpeTransfer message)
		{
			Client.Transfered = true;
			Client.SendDisconnectionNotification();
			WorldProvider.Dispose();

			string serverAddress = message.serverAddress;
			ushort port          = message.port;
			
			ThreadPool.QueueUserWorkItem(
				async o =>
				{
					var resolved = await JavaServerQueryProvider.ResolveHostnameAsync(serverAddress);

					if (!resolved.Success)
					{
						Log.Warn($"could not resolve hostname: {serverAddress}");
						return;
					}

					AlexInstance.ConnectToServer(
						new BedrockServerType(AlexInstance, AlexInstance.Services.GetService<XboxAuthService>()),
						new ServerConnectionDetails(new IPEndPoint(resolved.Result, port)), PlayerProfile);
				});
		}

		public void HandleMcpePlaySound(McpePlaySound message)
		{
			var coords = message.coordinates;

			if (!AlexInstance.AudioEngine.PlaySound(
				message.name, new Microsoft.Xna.Framework.Vector3(coords.X, coords.Y, coords.Z), message.pitch, message.volume))
			{
				Log.Warn($"Sound not found: {message.name}");
			}
		}

		public void HandleMcpeStopSound(McpeStopSound message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetTitle(McpeSetTitle message)
		{
			var titleComponent = WorldProvider?.TitleComponent;
			if (titleComponent == null)
				return;
			
			switch ((TitleType) message.type)
			{
				case TitleType.Clear:
					titleComponent.Hide();
					break;
				case TitleType.Reset:
					titleComponent.Reset();
					break;
				case TitleType.Title:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.SetTitle(message.text);
					titleComponent.Show();
					//titleComponent.Show();
					break;
				case TitleType.SubTitle:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.SetSubtitle(message.text);
					//titleComponent.Show();
					break;
				case TitleType.ActionBar:
					
					break;
				case TitleType.AnimationTimes:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.Show();
					break;
			}
	
			//UnhandledPackage(message);
		}

		public void HandleMcpeAddBehaviorTree(McpeAddBehaviorTree message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeStructureBlockUpdate(McpeStructureBlockUpdate message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeShowStoreOffer(McpeShowStoreOffer message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpePlayerSkin(McpePlayerSkin message)
		{
			if (Client.World.EntityManager.TryGet(message.uuid, out var entity) && entity is RemotePlayer player)
			{
				player.Skin = message.skin;
			}
		}

		public void HandleMcpeSubClientLogin(McpeSubClientLogin message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeInitiateWebSocketConnection(McpeInitiateWebSocketConnection message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetLastHurtBy(McpeSetLastHurtBy message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeBookEdit(McpeBookEdit message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeNpcRequest(McpeNpcRequest message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeModalFormRequest(McpeModalFormRequest message)
		{
			Form form = JsonConvert.DeserializeObject<Form>(
				message.data, new FormConverter(), new CustomElementConverter());

			Client.WorldProvider.FormManager.Show(message.formId, form);
			//UnhandledPackage(message);
		}

		public void HandleMcpeServerSettingsResponse(McpeServerSettingsResponse message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeShowProfile(McpeShowProfile message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetDefaultGameType(McpeSetDefaultGameType message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeRemoveObjective(McpeRemoveObjective message)
		{
			var scoreboard = WorldProvider?.ScoreboardView;
			if (scoreboard == null)
				return;
			
			scoreboard.RemoveObjective(message.objectiveName);
		}

		public void HandleMcpeSetDisplayObjective(McpeSetDisplayObjective message)
		{
			var scoreboard = WorldProvider?.ScoreboardView;
			if (scoreboard == null)
				return;
			
			switch (message.displaySlot)
			{
				case "sidebar":
					scoreboard.Clear();
					scoreboard.AddObjective(new ScoreboardObjective(message.objectiveName, message.displayName, message.sortOrder, message.criteriaName));
					//scoreboard.AddString(message.displayName);
					break;
			}
		}

		public void HandleMcpeSetScore(McpeSetScore message)
		{
			var scoreboard = WorldProvider?.ScoreboardView;
			if (scoreboard == null)
				return;
			
			foreach (var entry in message.entries)
			{
				if (entry is ScoreEntryChangeFakePlayer fakePlayer)
				{
					if (scoreboard.TryGetObjective(fakePlayer.ObjectiveName, out var objective))
					{
						objective.AddOrUpdate(fakePlayer.Id.ToString(), new ScoreboardEntry(fakePlayer.Id.ToString(), fakePlayer.Score, fakePlayer.CustomName));
					}
					//scoreboard.AddRow($"{fakePlayer.ObjectiveName}:{fakePlayer.Id}", fakePlayer.CustomName, entry.Score);
				}
				else if (entry is ScoreEntryChangePlayer player)
				{

					if (Client.World.EntityManager.TryGet(player.EntityId, out var playerEntity))
					{
						if (scoreboard.TryGetObjective(player.ObjectiveName, out var objective))
						{
							objective.AddOrUpdate(
								player.Id.ToString(),
								new ScoreboardEntry(player.Id.ToString(), player.Score, playerEntity.NameTag));
						}

						//scoreboard.AddRow($"{player.ObjectiveName}:{player.Id}", playerEntity.NameTag, player.Score);
					}
				}
				else if (entry is ScoreEntryRemove remove)
				{
					if (scoreboard.TryGetObjective(remove.ObjectiveName, out var objective))
					{
						objective.Remove(remove.Id.ToString());
					}

					//	scoreboard.Remove($"{remove.ObjectiveName}:{remove.Id}");
				}
			}
		}

		public void HandleMcpeLabTable(McpeLabTable message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
		{
			UnhandledPackage(message);
		}

        public void HandleMcpeSetScoreboardIdentity(McpeSetScoreboardIdentity message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateSoftEnum(McpeUpdateSoftEnum message)
		{
			UnhandledPackage(message);
		}
		
		public void HandleMcpeNetworkStackLatency(McpeNetworkStackLatency message)
		{
			if (message.unknownFlag == 1)
			{
				var response = McpeNetworkStackLatency.CreateObject();
				response.timestamp = message.timestamp;
				response.unknownFlag = 0;
				
				Client.SendPacket(response);
			}
		}

		public void HandleMcpeScriptCustomEvent(McpeScriptCustomEvent message)
		{
			UnhandledPackage(message);
		}

		public void HandleFtlCreatePlayer(FtlCreatePlayer message)
		{
			UnhandledPackage(message);
		}
		
		/// <inheritdoc />
		public void ReportPackets()
		{
			var entries = UnhandledPackets.ToArray();
			UnhandledPackets.Clear();
			
			foreach (var p in entries)
			{
				Log.Warn(
					$"Unhandled packet: {p.Value.type.FullName} * {p.Value.count} (0x{p.Key:x2})");
			}
		}
	}
}
