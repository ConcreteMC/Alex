using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using Alex.Blocks;
using Alex.Common.Resources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Entities.Components.Effects;
using Alex.Entities.Projectiles;
using Alex.Gui.Elements;
using Alex.Net.Bedrock.Packets;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using Alex.Services;
using Alex.Utils;
using Alex.Utils.Auth;
using Alex.Utils.Commands;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Bedrock;
using fNbt;
using Jose;
using Microsoft.Extensions.DependencyInjection;
using MiNET;
using MiNET.Net;
using MiNET.Particles;
using MiNET.UI;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using AnvilWorldProvider = Alex.Worlds.Singleplayer.AnvilWorldProvider;
using BlockCoordinates = Alex.Common.Utils.Vectors.BlockCoordinates;
using ChunkCoordinates = Alex.Common.Utils.Vectors.ChunkCoordinates;
using Color = Microsoft.Xna.Framework.Color;
using CommandProperty = Alex.Utils.Commands.CommandProperty;
using Dimension = Alex.Worlds.Dimension;
using Entity = Alex.Entities.Entity;
using MessageType = Alex.Common.Data.MessageType;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.Common.Utils.Vectors.PlayerLocation;

namespace Alex.Net.Bedrock
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

        private BedrockTransactionTracker TransactionTracker => Client.TransactionTracker;
        public BedrockClientPacketHandler(BedrockClient client, WorldProvider worldProvider, PlayerProfile profile, Alex alex, CancellationToken cancellationToken, ChunkProcessor chunkProcessor) //:
	       // base(client)
        {
	        Client = client;
	        AlexInstance = alex;
	        CancellationToken = cancellationToken;
	        WorldProvider = worldProvider;
	        PlayerProfile = profile;
	        
	        AnvilWorldProvider.LoadBlockConverter();

	        ChunkProcessor = chunkProcessor;
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

			if (Client.PlayerStatus == 1)
			{
				Client.ShowDisconnect("multiplayer.status.client_out_of_date", true, true, DisconnectReason.Network);
			}
			else if (Client.PlayerStatus == 2)
			{
				Client.ShowDisconnect("multiplayer.status.server_out_of_date", true, true, DisconnectReason.Network);
			}
			else if (Client.PlayerStatus == 3)
			{
				//Client.MarkAsInitialized();
			}
			else if (Client.PlayerStatus == 0)
			{
				Log.Info($"Play Status: Login success, reporting cache status as {(ChunkProcessor.Cache.Enabled ? "Enabled" : "Disabled")}");
				
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

       // private ResourcePackIds _resourcePackIds;
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
						rawMessage = $"{message.source}{ChatFormatting.Reset} {rawMessage}";
					}

					break;
				case 7: //Whisper
					rawMessage = $"{ChatFormatting.Italic}{message.source}{ChatFormatting.Reset}{ChatFormatting.Italic} whispered{ChatFormatting.Reset}: {rawMessage}";
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
			if (message.movementType > 0)
			{
				Log.Warn($"!!! Server uses server-authoritive movement, only client-auth is currently supported.");
			}
			
			try
			{
				Client.World.Player.EntityId = Client.EntityId = message.runtimeEntityId;
				Client.NetworkEntityId = message.entityIdSelf;

				Client.SpawnPoint = new Vector3(
					message.spawn.X, message.spawn.Y - Player.EyeLevel, message.spawn.Z); //message.spawn;

				Client.World.Dimension = (Dimension) message.dimension;

				Client.World?.UpdatePlayerPosition(
					new PlayerLocation(
						new Microsoft.Xna.Framework.Vector3(message.spawn.X, message.spawn.Y, message.spawn.Z),
						message.rotation.Y, message.rotation.Y, message.rotation.X), true);

				if (message.enableNewInventorySystem)
				{
					Client.EnableNewInventorySystem = true;
					Log.Info($"Using new inventory system.");
				}

				if (message.enableNewBlockBreakSystem)
				{
					Log.Info($"New blockbreak system?");
				}

				Client.World.Player.SetInventory(new BedrockInventory(46) {ReportTransaction = true});

				Client.World.Player.UpdateGamemode((GameMode) message.playerGamemode);

				foreach (var gr in message.gamerules)
				{
					Client.World.SetGameRule(gr);
				}

				ChunkProcessor.Itemstates = message.itemstates;
			}
			finally
			{
				Client.GameStarted = true;
				
				Client.RequestChunkRadius(AlexInstance.Options.AlexOptions.VideoOptions.RenderDistance.Value);
			}
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			var pos = new PlayerLocation(
				message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch)
			{
				OnGround = message.onGround
			};

			Client.World.UpdateEntityPosition(message.runtimeEntityId, pos, false, true, true, adjustForEntityHeight:true, teleport:message.mode == 2);
		}

		private void UpdateEntityAdventureFlags(Entity entity, uint flags, uint actionPermissions)
		{
			if (entity == null)
				return;
			
			/*entity.CanFly = ((flags & 0x40) == 0x40);
			entity.IsFlying = ((flags & 0x200) == 0x200);
			
			entity.IsWorldImmutable = ((flags & 0x01) == 0x01);
			entity.IsNoPvP = (flags & 0x02) == 0x02;
			entity.IsNoPvM = (flags & 0x04) == 0x04;
			entity.HasCollision = (flags & 0x80) != 0x80;*/
			
			entity.IsWorldImmutable = (flags & 0x01) != 0;
			entity.IsNoPvP = (flags & 0x02) != 0;
			entity.IsNoPvM = (flags & 0x04) != 0;
			entity.CanFly = (flags & 0x40) != 0;
			entity.HasCollision = (flags & 0x80) == 0;
			entity.IsFlying = (flags & 0x200) != 0;
		}

		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			var userId = BinaryPrimitives.ReverseEndianness(message.userId);
			Entity entity = null;

			if (userId == Client.World.Player.EntityId)
			{
				entity = Client.World.Player;
			}
			else if (!Client.World.TryGetEntity(userId, out entity))
			{
			
			}

			if (entity == null)
				return;
			
			UpdateEntityAdventureFlags(entity, message.flags, message.actionPermissions);
		}

		public void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
			if (_playerListPlayers.TryGetValue(message.uuid, out var entityData))
			{
				var remotePlayer = new RemotePlayer(Client.World, skin: entityData.Skin);
				remotePlayer.SetInventory(new BedrockInventory(46));
				
				remotePlayer.NameTag = entityData.Nametag;
				remotePlayer.UUID = entityData.Uuid;
				remotePlayer.EntityId = message.runtimeEntityId;
				remotePlayer.RenderLocation = remotePlayer.KnownPosition = new PlayerLocation(
					message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch) {OnGround = true};

				remotePlayer.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);

				if (message.item != null)
				{
					remotePlayer.Inventory.MainHand = message.item.ToAlexItem();
				}

				if (message.metadata != null)
				{
					remotePlayer.HandleMetadata(message.metadata);
				}

				UpdateEntityAdventureFlags(remotePlayer, message.flags, message.actionPermissions);

				Client.World.SpawnEntity(remotePlayer);

				if (entityData.AllowRemoval)
				{
					_playerListPlayers.TryRemove(entityData.Uuid, out _);
				}
				//entityData.RemoveIn(TimeSpan.Zero);
			}
			else
			{
				var identifier = "";

				if (!string.IsNullOrWhiteSpace(message.username))
				{
					identifier = message.username.Replace("\n", "");
				}

				Log.Warn(
					$"({message.ReliabilityHeader.ReliableMessageNumber} | {message.ReliabilityHeader.OrderingIndex} | {message.ReliabilityHeader.SequencingIndex}) Tried spawning invalid player: {identifier} (UUID: {message.uuid} EntityID: {message.runtimeEntityId}))");


				//_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
			}
		}

		private ConcurrentDictionary<MiNET.Utils.UUID, PlayerListData> _playerListPlayers =
			new ConcurrentDictionary<MiNET.Utils.UUID, PlayerListData>();
		
		public void HandleMcpePlayerList(McpePlayerList message)
		{
			if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					if (_playerListPlayers.TryAdd(
						r.ClientUuid, new PlayerListData(r.ClientUuid, r.Skin, r.DisplayName)))
					{
						Client.World.AddPlayerListItem(
							new PlayerListItem(
								r.ClientUuid, r.DisplayName, (GameMode) ((int) r.GameMode), -1));
					}
				}
			}
			else if (message.records is PlayerRemoveRecords removeRecords)
			{
				foreach (var r in removeRecords)
				{
					Client.World.RemovePlayerListItem(r.ClientUuid);

					if (_playerListPlayers.TryGetValue(r.ClientUuid, out var value))
					{
						value.RemoveIn(TimeSpan.FromMinutes(5));
					}
				}
			}

			foreach (var element in _playerListPlayers.Where(x => x.Value.ShouldRemove).ToArray())
			{
				_playerListPlayers.TryRemove(element.Key, out _);
			}
		}

		private class PlayerListData
		{
			public MiNET.Utils.Skins.Skin Skin { get; }
			public MiNET.Utils.UUID Uuid { get; }
			public string Nametag { get; }

			private DateTime RemovalTime { get; set; } = DateTime.MaxValue;
			public bool ShouldRemove => DateTime.UtcNow > RemovalTime;

			public bool AllowRemoval { get; set; } = false;
			public PlayerListData(MiNET.Utils.UUID uuid, MiNET.Utils.Skins.Skin skin, string nametag)
			{
				Skin = skin;
				Uuid = uuid;
				Nametag = nametag;
			}

			public void RemoveIn(TimeSpan time)
			{
				AllowRemoval = true;
				RemovalTime = DateTime.UtcNow + time;
			}
		}

		public void HandleMcpeAddEntity(McpeAddEntity message)
		{
			if (message.entityType.Contains("hive"))
			{
				Log.Info($"spawning: {message.entityType}");
			}
			Entity entity = EntityFactory.Create(message.entityType, null);

			if (entity == null)
			{
				Log.Warn($"Cannot spawn entity of type: {message.entityType}");
				return;
			}

			//var uuid = MiNETExtensions.FromEntityId(message.runtimeEntityId);
			entity.KnownPosition = new PlayerLocation(message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch);
			entity.KnownPosition.OnGround = false;
			
			entity.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);
			entity.EntityId = message.runtimeEntityId;
			entity.UUID = new UUID(Guid.NewGuid().ToByteArray());
			//	entity.Texture = texture2D;
			entity.SetInventory(new BedrockInventory(46));
			
			
			if (message.metadata != null)
				entity.HandleMetadata(message.metadata);

			if (message.attributes != null)
				entity.UpdateAttributes(message.attributes);
			//entity.ad

			if (message.links != null)
			{
				foreach (var link in message.links)
				{
					
				}
			}
			
			Client.World.SpawnEntity(entity);
		}

		//private ConcurrentDictionary<long, long> _entityMapping = new ConcurrentDictionary<long, long>();
		public void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			Client.World.DespawnEntity(message.entityIdSelf);
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
				//_entityMapping.TryAdd(message.entityIdSelf, message.runtimeEntityId);
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
					//Log.Warn($"Invalid TakeItemEntity request.");
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

			if (message.runtimeEntityId == Client.EntityId)
			{
				location.Y -= Player.EyeLevel;
				Client.World.UpdatePlayerPosition(location, true);
				return;
			}
			
			Client.World.UpdateEntityPosition(message.runtimeEntityId, location, false, true, true, false, true);
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
						
						Client.World.UpdateEntityPosition(message.runtimeEntityId, known, false, true, true, false, true);
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

		public void HandleMcpeSpawnParticleEffect(McpeSpawnParticleEffect message)
		{
			if (!AlexInstance.ParticleManager.SpawnParticle(
				message.particleName,
				new Microsoft.Xna.Framework.Vector3(message.position.X, message.position.Y, message.position.Z)))
			{
				Log.Warn($"Unknonw particle: {message.particleName}");
			}
			//UnhandledPackage(message);
        }

		private ConcurrentDictionary<string, int> _entityIdentifiers = new ConcurrentDictionary<string, int>();

		public void HandleMcpeAvailableEntityIdentifiers(McpeAvailableEntityIdentifiers message)
		{
			var idList = message.namedtag.NbtFile?.RootTag?["idlist"];

			if (idList != null && idList is NbtList ids)
			{
				foreach (NbtCompound tag in ids.ToArray<NbtCompound>())
				{
					if (tag.TryGet("id", out NbtString id) && tag.TryGet("rid", out NbtInt realId))
					{
						_entityIdentifiers[id.Value] = realId.Value;

						if (LoggingConstants.LogServerEntityDefinitions)
							Log.Debug($"Registered entity identifier: {id.Value}");
					}
				}
			}
			//UnhandledPackage(message);

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
			TransactionTracker.HandleResponse(message.responses);
			
			//UnhandledPackage(message);
		//	if (Client.World.Player.Inventory is ItemStackInventory itemStackInventory)
		//	{
			//	itemStackInventory.HandleResponses(message.responses);
		//	}
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
			/*Entity entity;
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
			}*/

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
			var msgId = message.eventId;

			if (msgId >= 0x4000 && msgId <= (0x4000 | (int) ParticleType.Sneeze)) //Got particle :)
			{
				var particleType = ((ParticleType) msgId -  0x4000);

				if (!AlexInstance.ParticleManager.SpawnParticle(
					particleType,
					new Microsoft.Xna.Framework.Vector3(message.position.X, message.position.Y, message.position.Z), message.data))
				{
					Log.Warn($"Unknown particle type: {particleType}");
				}

				return;
			}
			
			//if (!Client.World.Player.IsBreakingBlock)
			//	return;
			var blockcoords = (BlockCoordinates) new Microsoft.Xna.Framework.Vector3(
				message.position.X, message.position.Y, message.position.Z);

			switch (message.eventId)
			{
				case 3001:
					Client.World.SetRain(true);
					break;
				
				case 3002:
					Client.World.SetThunder(true);
					break;
				
				case 3003:
					Client.World.SetRain(false);
					break;
				
				case 3004:
					Client.World.SetThunder(false);
					break;
				
				case 3600: //Start blockbreak
					var ticksRequired = (double) ushort.MaxValue / message.data;

					if (blockcoords == Client.World.Player.TargetBlock)
						Client.World.Player.BreakTimeNeeded = ticksRequired;
					
					Client.World.AddOrUpdateBlockBreak(blockcoords, ticksRequired);
					break;
				case 3601: //Stop blockbreak
					Client.World.EndBreakBlock(blockcoords);
					break;
				case 3602: //Update blockcracking
					//Client.World.SetBlockBreakProgress(blockcoords);
					break;
			}
			
			/*if ((BlockCoordinates) new Microsoft.Xna.Framework.Vector3(
				message.position.X, message.position.Y, message.position.Z) == Client.World.Player.TargetBlock)
			{
				if (message.eventId == 3600)
				{
					var ticksRequired = (double) ushort.MaxValue / message.data;
					Client.World.Player.BreakTimeNeeded = ticksRequired;
				}
			}*/
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
				entity.HandleEntityEvent(message.eventId, message.data);
			}
			//UnhandledPackage(message);
		}

		public void HandleMcpeMobEffect(McpeMobEffect message)
		{
			Entity entity;

			if (Client.World.EntityManager.TryGet(message.runtimeEntityId, out entity))
			{
				Effect     effect     = null;
				EffectType effectType = (EffectType) message.effectId;
				switch (effectType)
				{
					case EffectType.Slowness:
						effect = new SlownessEffect();
						break;
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
						entity.Effects.AddOrUpdateEffect(effect);
						break;
				
					case 2: //Modify
						if (entity.Effects.TryGetEffect(effectType, out effect))
						{
							effect.Duration = message.duration;
							effect.Particles = message.particles;
							effect.Level = message.amplifier;
						
							entity.Effects.AddOrUpdateEffect(effect);
						}
						break;

					case 3: //Remove
						entity.Effects.RemoveEffect(effect.EffectId);
						break;
				}
			}
		}

		public EntityAttributes ConvertEntityAttributes(PlayerAttributes playerAttributes)
		{
			EntityAttributes attributes = new EntityAttributes();

			foreach (var attribute in playerAttributes)
			{
				attributes.TryAdd(
					attribute.Key,
					new EntityAttribute()
					{
						Name = attribute.Value.Name,
						Value = attribute.Value.Value,
						MaxValue = attribute.Value.MaxValue,
						MinValue = attribute.Value.MinValue
					});
			}
			//if (entity != null)
			return attributes;
		}

		public void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
		{
			Entity entity = null;
			
			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (message.runtimeEntityId == Client.EntityId)
					entity = Client.World.Player;
			}

			if (entity == null) return;

			entity.UpdateAttributes(ConvertEntityAttributes(message.attributes));
		}

		public void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			if (Client.EntityId == message.runtimeEntityId)
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

				if (slot >= 0 || slot < entity.Inventory.SlotCount - 1)
				{
					entity.Inventory.SetSlot(slot, item, true);
					//entity.Inventory[slot] = item;
					entity.Inventory.SelectedSlot = message.selectedSlot;
				}
				else
				{
					Log.Warn($"Inventory slot {slot} is out of range for entity: {entity.ToString()} (Min=0 Max={entity.Inventory.SlotCount})");
				}
			}

		//	UnhandledPackage(message);
		}

		public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			Entity entity = null;

			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.EntityId == message.runtimeEntityId)
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
				if (Client.EntityId == message.runtimeEntityId)
				{
					entity = Client.World.Player;
				}
			}

			if (entity == null)
				return;

			entity.HandleMetadata(message.metadata);
		}

		public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			var v = message.velocity;
			var velocity = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);

			Entity entity = null;
			//Client.World.TryGetEntity(message.runtimeEntityId, out entity);
			if (!Client.World.TryGetEntity(message.runtimeEntityId, out entity))
			{
				if (Client.EntityId == message.runtimeEntityId)
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
				Client.World.UpdatePlayerPosition(new PlayerLocation(message.x, message.y, message.z), true);

				if (Client.CanSpawn)
				{
					Client.SendMcpeMovePlayer(new PlayerLocation(message.x, message.y, message.z), 1);
				}

				Client.RequestChunkRadius(AlexInstance.Options.AlexOptions.VideoOptions.RenderDistance.Value);

				Client.ChangeDimensionResetEvent.Set();
			}
		}

		public void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			try
			{
				var windowId = message.windowId;
				var dialog = Client.World.InventoryManager.Show(Client.World.Player.Inventory, message.windowId, (ContainerType) message.type);
				dialog.TransactionTracker = TransactionTracker;

				dialog.OnContainerClose += (sender, args) =>
				{
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

			InventoryBase inventory  = null;
			//var 

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

						return;
					}
					else if (message.inventoryId == 124)
					{
						inventory = Client.World.Player.Inventory.UiInventory;
					}

					/*	if (message.inventoryId == 124)
						{
							startIndex = bi.slot
						}*/
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
					var adjustedIndex = startIndex + index;

					if (adjustedIndex > inventory.SlotCount - 1)
					{
						Log.Warn($"Slot {adjustedIndex} out of range for inventory {message.inventoryId} (Limit: {inventory.SlotCount})");
					}
					else
					{
						inventory.SetSlot(adjustedIndex, result, true);
					}

					//Log.Info($"Server container slot (slot={(startIndex + index)} inventory={message.inventoryId} stackId={result.StackID} uniqueId={slot.UniqueId}): {result.Name}");
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

			InventoryBase inventory = null;

			if (message.inventoryId == 124)
			{
				inventory = Client.World.Player.Inventory.UiInventory;
			}
			
			if (message.inventoryId == 0x00 //Inventory
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
				       //    Log.Info($"Server set single slot (slot={index} inventory={message.inventoryId} stackId={result.StackID} uniqueId={message.uniqueid}): {result.Name}");
			           }

			           break;
		           }

		           case 120:
		           {
			           if (inventory is BedrockInventory bi)
			           {
				           bi.SetSlot(bi.BootsSlot + index, result, true);
				         //  Log.Info($"Server set single slot (slot={index} inventory={message.inventoryId} stackId={result.StackID} uniqueId={message.uniqueid}): {result.Name}");
			           }

			           // inventory.SetSlot();
			           break;
		           }

		           case 124:
			         //  Log.Info($"Server set cursor: {result.Name}");
			           inventory.SetCursor(result, true);
			           break;
		           
		           case 0:
			         //  Log.Info($"Server set single slot (slot={index} inventory={message.inventoryId} stackId={result.StackID} uniqueId={message.uniqueid}): {result.Name}");
			           inventory.SetSlot(index, result, true);
			           break;
		           
		           default:
			          // Log.Warn($"Server tried to set single slot (slot={index} inventory={message.inventoryId} stackId={result.StackID} uniqueId={message.uniqueid}): {result.Name}");
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
			
			ChunkProcessor.HandleChunkData(cacheEnabled, blobs, subChunkCount, chunkData, cx, cz);
		}

		private object _changeDimensionLock = new object();
		public void HandleMcpeChangeDimension(McpeChangeDimension message)
		{
			//base.HandleMcpeChangeDimension(message);

			ThreadPool.QueueUserWorkItem(
				(o) =>
				{
					if (!Monitor.TryEnter(_changeDimensionLock))
						return;

					Client.WorldProvider.FormManager.CloseAll();
					var chunkCoords = new ChunkCoordinates(
						new PlayerLocation(Client.World.SpawnPoint.X, Client.World.SpawnPoint.Y, Client.World.SpawnPoint.Z));
			
					Client.World.Player.IsSpawned = false;
					bool cancelled = false;
					LoadingWorldScreen loadingWorldScreen = new LoadingWorldScreen()
					{
						ConnectingToServer = true,
						CancelAction = () =>
						{
							cancelled = true;
							Client.ShowDisconnect("Disconnect requested by user.", false, false, DisconnectReason.Unknown);
							Client.Close();
						}
					};

					AlexInstance.GuiManager.AddScreen(loadingWorldScreen);
					//	AlexInstance.GameStateManager.SetActiveState(loadingWorldState, true);
					loadingWorldScreen.UpdateProgress(LoadingState.LoadingChunks, 0);
					
					try
					{
						Client.ResetInitialized();
						
						World world = Client.World;

						//_entityMapping.Clear();
						
						WorldProvider?.BossBarContainer?.Reset();
						_bossBarMapping.Clear();
						
					//	WorldProvider.BossBarContainer?.Reset();
						world.ClearChunksAndEntities();


						//world.ChunkManager.ClearChunks();
						world.UpdatePlayerPosition(
							new PlayerLocation(message.position.X, message.position.Y, message.position.Z), true);


						//foreach (var loadedChunk in provider.LoadedChunks)
						//{
						//	provider.UnloadChunk(loadedChunk);
						//}
						McpePlayerAction action = McpePlayerAction.CreateObject();
						action.runtimeEntityId = Client.EntityId;
						action.actionId = (int) PlayerAction.DimensionChangeAck;
						Client.SendPacket(action);

						int percentage = 0;
						bool ready = false;
						int previousPercentage = 0;

						LoadingState state = LoadingState.LoadingChunks;
						do
						{
							if (cancelled || Client?.World?.Player?.KnownPosition == null || !Client.IsConnected)
							{
								break;
							}
							
							chunkCoords = new ChunkCoordinates(
								new PlayerLocation(
									Client.World.Player.KnownPosition.X, Client.World.Player.KnownPosition.Y,
									Client.World.Player.KnownPosition.Z));


							double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
							var target = radiusSquared;

							percentage = (int) ((100 / target) * world.ChunkManager.ChunkCount);

							if (percentage != previousPercentage)
							{
								loadingWorldScreen.UpdateProgress(state, percentage);
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


							if (Client.CanSpawn)
							{
								break;
							}

							//	}
							//	else
							//	{
							//	await Task.Delay(50);
							//}
						} while (Client.IsConnected);
						Client.MarkAsInitialized();
						//AlexInstance.GameStateManager.Back();

						var p = Client.World.Player.KnownPosition;

						//Client.SendMcpeMovePlayer(p, 1);
						//Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(Client.World.Player.KnownPosition.X, Client.World.Player.KnownPosition.Y, Client.World.Player.KnownPosition.Z), false);

						Client.World.Player.IsSpawned = true;
					}
					finally
					{
						AlexInstance.GuiManager.RemoveScreen(loadingWorldScreen);
						Monitor.Exit(_changeDimensionLock);
						//Interlocked.Decrement(ref _changeDimensionLock);
					}
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
			Log.Info(
				$"Received chunkradius. Requested={AlexInstance.Options.AlexOptions.VideoOptions.RenderDistance.Value} Received={message.chunkRadius}");
			//if (message.chunkRadius <= 4)
			//	return;
			
			Client.ChunkRadius = message.chunkRadius;

			//Client.SendMcpeMovePlayer(new MiNET.Utils.PlayerLocation(Client.SpawnPoint), false);
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

		private ConcurrentDictionary<long, MiNET.Utils.UUID> _bossBarMapping = new ConcurrentDictionary<long, MiNET.Utils.UUID>();
		public void HandleMcpeBossEvent(McpeBossEvent message)
		{
			var container = WorldProvider.BossBarContainer;

			if (container == null)
				return;

			switch ((McpeBossEvent.Type) message.eventType)
			{
				case McpeBossEvent.Type.AddBoss:

					_bossBarMapping.GetOrAdd(message.bossEntityId, l =>
					{
						MiNET.Utils.UUID uuid;

						var text = message.title;
						var health = message.healthPercent;
						
						if (Client.World.TryGetEntity(message.bossEntityId, out var entity))
						{
							if (string.IsNullOrWhiteSpace(text))
								text = entity.NameTag;
							//uuid = entity.UUID;
						//	health = entity.HealthManager.Health;
						}
						//else
						//{
							uuid = new MiNET.Utils.UUID(Guid.NewGuid().ToByteArray());
						//}

						/*if (string.IsNullOrWhiteSpace(text))
						{
							if (Client.World.TryGetEntity(message.bossEntityId, out var entity))
							{
								text = entity.NameTag;
							}
						}*/
						Microsoft.Xna.Framework.Vector3 customColor = AlexInstance.GuiRenderer.GetGlobalOrDefault("$boss_health_bar_color", Microsoft.Xna.Framework.Vector3.Zero);
						
						container.Add(
							uuid, text, health, BossBarPacket.BossBarColor.Pink,
							BossBarPacket.BossBarDivisions.None, 0, customColor == Microsoft.Xna.Framework.Vector3.Zero ? (Color?) null : new Color(customColor));
						
						return uuid;
					});
					//var uuid = new MiNET.Utils.UUID(Guid.NewGuid().ToByteArray());
					
					break;

				case McpeBossEvent.Type.AddPlayer:
					break;

				case McpeBossEvent.Type.RemoveBoss:
				{
					if (_bossBarMapping.TryRemove(message.bossEntityId, out var uuid))
					{
						container.Remove(uuid);
					}
				}
					break;

				case McpeBossEvent.Type.RemovePlayer:
					break;

				case McpeBossEvent.Type.UpdateProgress:
				{
					if (_bossBarMapping.TryGetValue(message.bossEntityId, out var uuid))
						container.UpdateHealth(uuid, message.healthPercent);
				}
					break;

				case McpeBossEvent.Type.UpdateName:
				{
					if (_bossBarMapping.TryGetValue(message.bossEntityId, out var uuid))
						container.UpdateTitle(uuid, message.title);
				}
					break;

				case McpeBossEvent.Type.UpdateOptions:
					break;

				case McpeBossEvent.Type.UpdateStyle:
				//	if (_bossBarMapping.TryGetValue(message.bossEntityId, out var uuid))
				//		container.UpdateStyle(uuid, message.color, message.overlay);
					break;
			}
		}

		public void HandleMcpeShowCredits(McpeShowCredits message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeAvailableCommands(McpeAvailableCommands message)
		{
			if (Client.CommandProvider is BedrockCommandProvider bcp)
			{
				bcp.Reset();
				foreach (var cmd in message.CommandSet)
				{
					foreach (var version in cmd.Value.Versions)
					{
						foreach (var overload in version.Overloads)
						{
							Command c = new Command(cmd.Key);
							c.Description = version.Description;
							
							foreach (var param in overload.Value.Input.Parameters)
							{
								if (param.Type == "stringenum")
								{
									string enumName = param.EnumType;
									string[] options = null;
									if (param.EnumValues != null)
									{
										options = param.EnumValues;
									}

									switch (param.EnumType)
									{
										case "":
											break;
									}
									
									c.Properties.Add(new EnumCommandProperty(param.Name, !param.Optional, options, enumName));
								}
								else if (param.Type == "target")
								{
									c.Properties.Add(new TargetCommandProperty(param.Name, !param.Optional));
								}
								else if (param.Type == "rawtext")
								{
									c.Properties.Add(new TextCommandProperty(param.Name, !param.Optional));
								}
								else if (param.Type == "int")
								{
									c.Properties.Add(new IntCommandProperty(param.Name, !param.Optional));
								}
								else if (param.Type == "float")
								{
									c.Properties.Add(new FloatCommandProperty(param.Name, !param.Optional));
								}
								else if (param.Type == "string")
								{
									c.Properties.Add(new TextCommandProperty(param.Name, !param.Optional));
								}
								else
								{
									Log.Debug($"Unknown parameter type: {param.Type} (name: {param.Name})");
									c.Properties.Add(new CommandProperty(param.Name, !param.Optional));
								}
								
								//CommandProperty cp = new CommandProperty(param.Name, !param.Optional);
								//
								//c.Properties.Add(cp);
							}
							
							bcp.Register(c);
						}
					}
				}
				
				Log.Info($"Registered {bcp.Count} commands.");
			}
			// Client.LoadCommands(message.CommandSet);
			//UnhandledPackage(message);
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

		public void HandleMcpeLevelSoundEventOld(McpeLevelSoundEventOld message)
		{
			PlaySound(message.soundId, message.blockId, message.entityType.ToString(), message.position, message.isGlobal);
		}
		
		public void HandleMcpeLevelSoundEventV2(McpeLevelSoundEventV2 message)
		{
			PlaySound(message.soundId, message.blockId, message.entityType, message.position, message.isGlobal);
		}

		private void PlaySound(uint soundId, int blockId, string entityTypeId, Vector3 position, bool isGlobal)
		{
			string sound = null;
			string soundEvent = null;
			string soundCategory = null;
			string blockName = null;
			
			float pitch = 1f;
			float volume = 1f;
			SoundEvent se = null;
			SoundBindingsCollection collection = null;
			
			if (TryConvertSoundIdToMapping(soundId, out soundEvent))
			{
				soundEvent = soundEvent.ToLower();

				//if (!string.IsNullOrWhiteSpace(message.entityType))
				//{
					string entityType = string.IsNullOrWhiteSpace(entityTypeId) ? null : new ResourceLocation(entityTypeId).Path;
					bool useEntityType = blockId == -1 && !string.IsNullOrWhiteSpace(entityType);
					var blockstate = BlockFactory.GetBlockState((uint) blockId);
					soundCategory = blockstate == null ? null : blockstate.Block.BlockMaterial.SoundCategory;
					blockName = blockstate == null ? null : new ResourceLocation(blockstate.Name).Path;


					foreach (var resourcePack in Alex.Instance.Resources.ActiveBedrockResources)
					{
						if (!string.IsNullOrWhiteSpace(sound))
							break;
						
						if (se != null)
							break;

						if (resourcePack.SoundBindings == null)
							continue;

						collection = resourcePack.SoundBindings;

						SoundBinding soundBinding;
						if (!string.IsNullOrWhiteSpace(entityType))
						{
							if (collection.EntitySounds.Entities.TryGetValue(entityType, out soundBinding))
							{
								if (soundBinding.Events.TryGetValue(soundEvent, out se))
								{
									if (!string.IsNullOrWhiteSpace(se.Sound))
									{
										sound = se.Sound;
									}
								}
							}
						}

						if (!string.IsNullOrWhiteSpace(blockName))
						{
							if (collection.BlockSounds.TryGetValue(blockName, out soundBinding)
							    || collection.InteractiveSounds.BlockSounds.TryGetValue(blockName, out soundBinding))
							{
								if (soundBinding.Events.TryGetValue(soundEvent, out se))
								{
									if (!string.IsNullOrWhiteSpace(se.Sound))
									{
										sound = se.Sound;
									}
								}
							}
						}

						if (!string.IsNullOrWhiteSpace(soundCategory))
						{
							if (collection.BlockSounds.TryGetValue(soundCategory, out soundBinding)
							    || collection.InteractiveSounds.BlockSounds.TryGetValue(
								    soundCategory, out soundBinding))
							{
								if (soundBinding.Events.TryGetValue(soundEvent, out se))
								{
									if (!string.IsNullOrWhiteSpace(se.Sound))
									{
										sound = se.Sound;
									}
								}
							}
						}

						if (collection.EntitySounds.Defaults.Events.TryGetValue(soundEvent, out se))
						{
							if (!string.IsNullOrWhiteSpace(se.Sound))
							{
								sound = se.Sound;
							}
						}
					}
					//}
			}
			
			if (string.IsNullOrWhiteSpace(sound))
			{
				Log.Debug($"Failed to translate sound with id: {soundId} (Sound={sound}, SoundEvent={soundEvent}, blockid={blockId}, entityType={entityTypeId}, block={blockName}, soundCat={soundCategory})");
				return;
			}
 
			if (!AlexInstance.AudioEngine.PlaySound(
				sound, new Microsoft.Xna.Framework.Vector3(position.X, position.Y, position.Z),
				pitch, volume, isGlobal))
			{
				Log.Debug($"Could not play sound: {sound}");
			}
		}

		public void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
		{
			PlaySound(message.soundId, message.blockId, message.entityType, message.position, message.isGlobal);
			//UnhandledPackage(message);
		}

		#region SoundConversion

		private bool TryConvertSoundId(uint soundId, out string sound)
		{
			sound = null;
			return false;
		}
		
		private bool TryConvertSoundIdToMapping(uint soundId, out string sound)
		{
			switch (soundId)
			{
				case 0:
				{
					sound = "ITEM_USE_ON";

					return true;
				}

				case 1:
				{
					sound = "HIT";

					return true;
				}

				case 2:
				{
					sound = "STEP";

					return true;
				}

				case 3:
				{
					sound = "FLY";

					return true;
				}

				case 4:
				{
					sound = "JUMP";

					return true;
				}

				case 5:
				{
					sound = "BREAK";

					return true;
				}

				case 6:
				{
					sound = "PLACE";

					return true;
				}

				case 7:
				{
					sound = "HEAVY_STEP";

					return true;
				}

				case 8:
				{
					sound = "GALLOP";

					return true;
				}

				case 9:
				{
					sound = "FALL";

					return true;
				}

				case 10:
				{
					sound = "AMBIENT";

					return true;
				}

				case 11:
				{
					sound = "AMBIENT_BABY";

					return true;
				}

				case 12:
				{
					sound = "AMBIENT_IN_WATER";

					return true;
				}

				case 13:
				{
					sound = "BREATHE";

					return true;
				}

				case 14:
				{
					sound = "DEATH";

					return true;
				}

				case 15:
				{
					sound = "DEATH_IN_WATER";

					return true;
				}

				case 16:
				{
					sound = "DEATH_TO_ZOMBIE";

					return true;
				}

				case 17:
				{
					sound = "HURT";

					return true;
				}

				case 18:
				{
					sound = "HURT_IN_WATER";

					return true;
				}

				case 19:
				{
					sound = "MAD";

					return true;
				}

				case 20:
				{
					sound = "BOOST";

					return true;
				}

				case 21:
				{
					sound = "BOW";

					return true;
				}

				case 22:
				{
					sound = "SQUISH_BIG";

					return true;
				}

				case 23:
				{
					sound = "SQUISH_SMALL";

					return true;
				}

				case 24:
				{
					sound = "FALL_BIG";

					return true;
				}

				case 25:
				{
					sound = "FALL_SMALL";

					return true;
				}

				case 26:
				{
					sound = "SPLASH";

					return true;
				}

				case 27:
				{
					sound = "FIZZ";

					return true;
				}

				case 28:
				{
					sound = "FLAP";

					return true;
				}

				case 29:
				{
					sound = "SWIM";

					return true;
				}

				case 30:
				{
					sound = "DRINK";

					return true;
				}

				case 31:
				{
					sound = "EAT";

					return true;
				}

				case 32:
				{
					sound = "TAKEOFF";

					return true;
				}

				case 33:
				{
					sound = "SHAKE";

					return true;
				}

				case 34:
				{
					sound = "PLOP";

					return true;
				}

				case 35:
				{
					sound = "LAND";

					return true;
				}

				case 36:
				{
					sound = "SADDLE";

					return true;
				}

				case 37:
				{
					sound = "ARMOR";

					return true;
				}

				case 38:
				{
					sound = "MOB_ARMOR_STAND_PLACE";

					return true;
				}

				case 39:
				{
					sound = "ADD_CHEST";

					return true;
				}

				case 40:
				{
					sound = "THROW";

					return true;
				}

				case 41:
				{
					sound = "ATTACK";

					return true;
				}

				case 42:
				{
					sound = "ATTACK_NODAMAGE";

					return true;
				}

				case 43:
				{
					sound = "ATTACK_STRONG";

					return true;
				}

				case 44:
				{
					sound = "WARN";

					return true;
				}

				case 45:
				{
					sound = "SHEAR";

					return true;
				}

				case 46:
				{
					sound = "MILK";

					return true;
				}

				case 47:
				{
					sound = "THUNDER";

					return true;
				}

				case 48:
				{
					sound = "EXPLODE";

					return true;
				}

				case 49:
				{
					sound = "FIRE";

					return true;
				}

				case 50:
				{
					sound = "IGNITE";

					return true;
				}

				case 51:
				{
					sound = "FUSE";

					return true;
				}

				case 52:
				{
					sound = "STARE";

					return true;
				}

				case 53:
				{
					sound = "SPAWN";

					return true;
				}

				case 54:
				{
					sound = "SHOOT";

					return true;
				}

				case 55:
				{
					sound = "BREAK_BLOCK";

					return true;
				}

				case 56:
				{
					sound = "LAUNCH";

					return true;
				}

				case 57:
				{
					sound = "BLAST";

					return true;
				}

				case 58:
				{
					sound = "LARGE_BLAST";

					return true;
				}

				case 59:
				{
					sound = "TWINKLE";

					return true;
				}

				case 60:
				{
					sound = "REMEDY";

					return true;
				}

				case 61:
				{
					sound = "UNFECT";

					return true;
				}

				case 62:
				{
					sound = "LEVELUP";

					return true;
				}

				case 63:
				{
					sound = "BOW_HIT";

					return true;
				}

				case 64:
				{
					sound = "BULLET_HIT";

					return true;
				}

				case 65:
				{
					sound = "EXTINGUISH_FIRE";

					return true;
				}

				case 66:
				{
					sound = "ITEM_FIZZ";

					return true;
				}

				case 67:
				{
					sound = "CHEST_OPEN";

					return true;
				}

				case 68:
				{
					sound = "CHEST_CLOSED";

					return true;
				}

				case 69:
				{
					sound = "SHULKERBOX_OPEN";

					return true;
				}

				case 70:
				{
					sound = "SHULKERBOX_CLOSED";

					return true;
				}

				case 71:
				{
					sound = "ENDERCHEST_OPEN";

					return true;
				}

				case 72:
				{
					sound = "ENDERCHEST_CLOSED";

					return true;
				}

				case 73:
				{
					sound = "POWER_ON";

					return true;
				}

				case 74:
				{
					sound = "POWER_OFF";

					return true;
				}

				case 75:
				{
					sound = "ATTACH";

					return true;
				}

				case 76:
				{
					sound = "DETACH";

					return true;
				}

				case 77:
				{
					sound = "DENY";

					return true;
				}

				case 78:
				{
					sound = "TRIPOD";

					return true;
				}

				case 79:
				{
					sound = "POP";

					return true;
				}

				case 80:
				{
					sound = "DROP_SLOT";

					return true;
				}

				case 81:
				{
					sound = "NOTE";

					return true;
				}

				case 82:
				{
					sound = "THORNS";

					return true;
				}

				case 83:
				{
					sound = "PISTON_IN";

					return true;
				}

				case 84:
				{
					sound = "PISTON_OUT";

					return true;
				}

				case 85:
				{
					sound = "PORTAL";

					return true;
				}

				case 86:
				{
					sound = "WATER";

					return true;
				}

				case 87:
				{
					sound = "LAVA_POP";

					return true;
				}

				case 88:
				{
					sound = "LAVA";

					return true;
				}

				case 89:
				{
					sound = "BURP";

					return true;
				}

				case 90:
				{
					sound = "BUCKET_FILL_WATER";

					return true;
				}

				case 91:
				{
					sound = "BUCKET_FILL_LAVA";

					return true;
				}

				case 92:
				{
					sound = "BUCKET_EMPTY_WATER";

					return true;
				}

				case 93:
				{
					sound = "BUCKET_EMPTY_LAVA";

					return true;
				}

				case 94:
				{
					sound = "ARMOR_EQUIP_CHAIN";

					return true;
				}

				case 95:
				{
					sound = "ARMOR_EQUIP_DIAMOND";

					return true;
				}

				case 96:
				{
					sound = "ARMOR_EQUIP_GENERIC";

					return true;
				}

				case 97:
				{
					sound = "ARMOR_EQUIP_GOLD";

					return true;
				}

				case 98:
				{
					sound = "ARMOR_EQUIP_IRON";

					return true;
				}

				case 99:
				{
					sound = "ARMOR_EQUIP_LEATHER";

					return true;
				}

				case 100:
				{
					sound = "ARMOR_EQUIP_ELYTRA";

					return true;
				}

				case 101:
				{
					sound = "RECORD_";

					return true;
				}

				case 102:
				{
					sound = "RECORD_CAT";

					return true;
				}

				case 103:
				{
					sound = "RECORD_BLOCKS";

					return true;
				}

				case 104:
				{
					sound = "RECORD_CHIRP";

					return true;
				}

				case 105:
				{
					sound = "RECORD_FAR";

					return true;
				}

				case 106:
				{
					sound = "RECORD_MALL";

					return true;
				}

				case 107:
				{
					sound = "RECORD_MELLOHI";

					return true;
				}

				case 108:
				{
					sound = "RECORD_STAL";

					return true;
				}

				case 109:
				{
					sound = "RECORD_STRAD";

					return true;
				}

				case 110:
				{
					sound = "RECORD_WARD";

					return true;
				}

				case 111:
				{
					sound = "RECORD_";

					return true;
				}

				case 112:
				{
					sound = "RECORD_WAIT";

					return true;
				}

				case 113:
				{
					sound = "STOP_RECORD";

					return true;
				}

				case 114:
				{
					sound = "FLOP";

					return true;
				}

				case 115:
				{
					sound = "ELDERGUARDIAN_CURSE";

					return true;
				}

				case 116:
				{
					sound = "MOB_WARNING";

					return true;
				}

				case 117:
				{
					sound = "MOB_WARNING_BABY";

					return true;
				}

				case 118:
				{
					sound = "TELEPORT";

					return true;
				}

				case 119:
				{
					sound = "SHULKER_OPEN";

					return true;
				}

				case 120:
				{
					sound = "SHULKER_CLOSE";

					return true;
				}

				case 121:
				{
					sound = "HAGGLE";

					return true;
				}

				case 122:
				{
					sound = "HAGGLE_YES";

					return true;
				}

				case 123:
				{
					sound = "HAGGLE_NO";

					return true;
				}

				case 124:
				{
					sound = "HAGGLE_IDLE";

					return true;
				}

				case 125:
				{
					sound = "CHORUSGROW";

					return true;
				}

				case 126:
				{
					sound = "CHORUSDEATH";

					return true;
				}

				case 127:
				{
					sound = "GLASS";

					return true;
				}

				case 128:
				{
					sound = "POTION_BREWED";

					return true;
				}

				case 129:
				{
					sound = "CAST_SPELL";

					return true;
				}

				case 130:
				{
					sound = "PREPARE_ATTACK";

					return true;
				}

				case 131:
				{
					sound = "PREPARE_SUMMON";

					return true;
				}

				case 132:
				{
					sound = "PREPARE_WOLOLO";

					return true;
				}

				case 133:
				{
					sound = "FANG";

					return true;
				}

				case 134:
				{
					sound = "CHARGE";

					return true;
				}

				case 135:
				{
					sound = "CAMERA_TAKE_PICTURE";

					return true;
				}

				case 136:
				{
					sound = "LEASHKNOT_PLACE";

					return true;
				}

				case 137:
				{
					sound = "LEASHKNOT_BREAK";

					return true;
				}

				case 138:
				{
					sound = "GROWL";

					return true;
				}

				case 139:
				{
					sound = "WHINE";

					return true;
				}

				case 140:
				{
					sound = "PANT";

					return true;
				}

				case 141:
				{
					sound = "PURR";

					return true;
				}

				case 142:
				{
					sound = "PURREOW";

					return true;
				}

				case 143:
				{
					sound = "DEATH_MIN_VOLUME";

					return true;
				}

				case 144:
				{
					sound = "DEATH_MID_VOLUME";

					return true;
				}

				case 145:
				{
					sound = "IMITATE_BLAZE";

					return true;
				}

				case 146:
				{
					sound = "IMITATE_CAVE_SPIDER";

					return true;
				}

				case 147:
				{
					sound = "IMITATE_CREEPER";

					return true;
				}

				case 148:
				{
					sound = "IMITATE_ELDER_GUARDIAN";

					return true;
				}

				case 149:
				{
					sound = "IMITATE_ENDER_DRAGON";

					return true;
				}

				case 150:
				{
					sound = "IMITATE_ENDERMAN";

					return true;
				}

				case 152:
				{
					sound = "IMITATE_EVOCATION_ILLAGER";

					return true;
				}

				case 153:
				{
					sound = "IMITATE_GHAST";

					return true;
				}

				case 154:
				{
					sound = "IMITATE_HUSK";

					return true;
				}

				case 155:
				{
					sound = "IMITATE_ILLUSION_ILLAGER";

					return true;
				}

				case 156:
				{
					sound = "IMITATE_MAGMA_CUBE";

					return true;
				}

				case 157:
				{
					sound = "IMITATE_POLAR_BEAR";

					return true;
				}

				case 158:
				{
					sound = "IMITATE_SHULKER";

					return true;
				}

				case 159:
				{
					sound = "IMITATE_SILVERFISH";

					return true;
				}

				case 160:
				{
					sound = "IMITATE_SKELETON";

					return true;
				}

				case 161:
				{
					sound = "IMITATE_SLIME";

					return true;
				}

				case 162:
				{
					sound = "IMITATE_SPIDER";

					return true;
				}

				case 163:
				{
					sound = "IMITATE_STRAY";

					return true;
				}

				case 164:
				{
					sound = "IMITATE_VEX";

					return true;
				}

				case 165:
				{
					sound = "IMITATE_VINDICATION_ILLAGER";

					return true;
				}

				case 166:
				{
					sound = "IMITATE_WITCH";

					return true;
				}

				case 167:
				{
					sound = "IMITATE_WITHER";

					return true;
				}

				case 168:
				{
					sound = "IMITATE_WITHER_SKELETON";

					return true;
				}

				case 169:
				{
					sound = "IMITATE_WOLF";

					return true;
				}

				case 170:
				{
					sound = "IMITATE_ZOMBIE";

					return true;
				}

				case 171:
				{
					sound = "IMITATE_ZOMBIE_PIGMAN";

					return true;
				}

				case 172:
				{
					sound = "IMITATE_ZOMBIE_VILLAGER";

					return true;
				}

				case 173:
				{
					sound = "BLOCK_END_PORTAL_FRAME_FILL";

					return true;
				}

				case 174:
				{
					sound = "BLOCK_END_PORTAL_SPAWN";

					return true;
				}

				case 175:
				{
					sound = "RANDOM_ANVIL_USE";

					return true;
				}

				case 176:
				{
					sound = "BOTTLE_DRAGONBREATH";

					return true;
				}

				case 177:
				{
					sound = "PORTAL_TRAVEL";

					return true;
				}

				case 178:
				{
					sound = "ITEM_TRIDENT_HIT";

					return true;
				}

				case 179:
				{
					sound = "ITEM_TRIDENT_RETURN";

					return true;
				}

				case 180:
				{
					sound = "ITEM_TRIDENT_RIPTIDE_";

					return true;
				}

				case 181:
				{
					sound = "ITEM_TRIDENT_RIPTIDE_";

					return true;
				}

				case 182:
				{
					sound = "ITEM_TRIDENT_RIPTIDE_";

					return true;
				}

				case 183:
				{
					sound = "ITEM_TRIDENT_THROW";

					return true;
				}

				case 184:
				{
					sound = "ITEM_TRIDENT_THUNDER";

					return true;
				}

				case 185:
				{
					sound = "ITEM_TRIDENT_HIT_GROUND";

					return true;
				}

				case 186:
				{
					sound = "DEFAULT";

					return true;
				}

				case 187:
				{
					sound = "BLOCK_FLETCHING_TABLE_USE";

					return true;
				}

				case 188:
				{
					sound = "ELEMCONSTRUCT_OPEN";

					return true;
				}

				case 189:
				{
					sound = "ICEBOMB_HIT";

					return true;
				}

				case 190:
				{
					sound = "BALLOONPOP";

					return true;
				}

				case 191:
				{
					sound = "LT_REACTION_ICEBOMB";

					return true;
				}

				case 192:
				{
					sound = "LT_REACTION_BLEACH";

					return true;
				}

				case 193:
				{
					sound = "LT_REACTION_EPASTE";

					return true;
				}

				case 194:
				{
					sound = "LT_REACTION_EPASTE";

					return true;
				}

				case 199:
				{
					sound = "LT_REACTION_FERTILIZER";

					return true;
				}

				case 200:
				{
					sound = "LT_REACTION_FIREBALL";

					return true;
				}

				case 201:
				{
					sound = "LT_REACTION_MGSALT";

					return true;
				}

				case 202:
				{
					sound = "LT_REACTION_MISCFIRE";

					return true;
				}

				case 203:
				{
					sound = "LT_REACTION_FIRE";

					return true;
				}

				case 204:
				{
					sound = "LT_REACTION_MISCEXPLOSION";

					return true;
				}

				case 205:
				{
					sound = "LT_REACTION_MISCMYSTICAL";

					return true;
				}

				case 206:
				{
					sound = "LT_REACTION_MISCMYSTICAL";

					return true;
				}

				case 207:
				{
					sound = "LT_REACTION_PRODUCT";

					return true;
				}

				case 208:
				{
					sound = "SPARKLER_USE";

					return true;
				}

				case 209:
				{
					sound = "GLOWSTICK_USE";

					return true;
				}

				case 210:
				{
					sound = "SPARKLER_ACTIVE";

					return true;
				}

				case 211:
				{
					sound = "CONVERT_TO_DROWNED";

					return true;
				}

				case 212:
				{
					sound = "BUCKET_FILL_FISH";

					return true;
				}

				case 213:
				{
					sound = "BUCKET_EMPTY_FISH";

					return true;
				}

				case 214:
				{
					sound = "BUBBLE_UP";

					return true;
				}

				case 215:
				{
					sound = "BUBBLE_DOWN";

					return true;
				}

				case 216:
				{
					sound = "BUBBLE_POP";

					return true;
				}

				case 217:
				{
					sound = "BUBBLE_UPINSIDE";

					return true;
				}

				case 218:
				{
					sound = "BUBBLE_DOWNINSIDE";

					return true;
				}

				case 219:
				{
					sound = "HURT_BABY";

					return true;
				}

				case 220:
				{
					sound = "DEATH_BABY";

					return true;
				}

				case 221:
				{
					sound = "STEP_BABY";

					return true;
				}

				case 223:
				{
					sound = "BORN";

					return true;
				}

				case 224:
				{
					sound = "BLOCK_TURTLE_EGG_BREAK";

					return true;
				}

				case 225:
				{
					sound = "BLOCK_TURTLE_EGG_CRACK";

					return true;
				}

				case 226:
				{
					sound = "BLOCK_TURTLE_EGG_HATCH";

					return true;
				}

				case 227:
				{
					sound = "LAY_EGG";

					return true;
				}

				case 228:
				{
					sound = "BLOCK_TURTLE_EGG_ATTACK";

					return true;
				}

				case 229:
				{
					sound = "BEACON_ACTIVATE";

					return true;
				}

				case 230:
				{
					sound = "BEACON_AMBIENT";

					return true;
				}

				case 231:
				{
					sound = "BEACON_DEACTIVATE";

					return true;
				}

				case 232:
				{
					sound = "BEACON_POWER";

					return true;
				}

				case 233:
				{
					sound = "CONDUIT_ACTIVATE";

					return true;
				}

				case 234:
				{
					sound = "CONDUIT_AMBIENT";

					return true;
				}

				case 235:
				{
					sound = "CONDUIT_ATTACK";

					return true;
				}

				case 236:
				{
					sound = "CONDUIT_DEACTIVATE";

					return true;
				}

				case 237:
				{
					sound = "CONDUIT_SHORT";

					return true;
				}

				case 238:
				{
					sound = "SWOOP";

					return true;
				}

				case 239:
				{
					sound = "BLOCK_BAMBOO_SAPLING_PLACE";

					return true;
				}

				case 240:
				{
					sound = "PRESNEEZE";

					return true;
				}

				case 241:
				{
					sound = "SNEEZE";

					return true;
				}

				case 242:
				{
					sound = "AMBIENT_TAME";

					return true;
				}

				case 243:
				{
					sound = "SCARED";

					return true;
				}

				case 244:
				{
					sound = "BLOCK_SCAFFOLDING_CLIMB";

					return true;
				}

				case 245:
				{
					sound = "CROSSBOW_LOADING_START";

					return true;
				}

				case 246:
				{
					sound = "CROSSBOW_LOADING_MIDDLE";

					return true;
				}

				case 247:
				{
					sound = "CROSSBOW_LOADING_END";

					return true;
				}

				case 248:
				{
					sound = "CROSSBOW_SHOOT";

					return true;
				}

				case 249:
				{
					sound = "CROSSBOW_QUICK_CHARGE_START";

					return true;
				}

				case 250:
				{
					sound = "CROSSBOW_QUICK_CHARGE_MIDDLE";

					return true;
				}

				case 251:
				{
					sound = "CROSSBOW_QUICK_CHARGE_END";

					return true;
				}

				case 252:
				{
					sound = "AMBIENT_AGGRESSIVE";

					return true;
				}

				case 253:
				{
					sound = "AMBIENT_WORRIED";

					return true;
				}

				case 254:
				{
					sound = "CANT_BREED";

					return true;
				}

				case 255:
				{
					sound = "ITEM_SHIELD_BLOCK";

					return true;
				}

				case 256:
				{
					sound = "ITEM_BOOK_PUT";

					return true;
				}

				case 257:
				{
					sound = "BLOCK_GRINDSTONE_USE";

					return true;
				}

				case 258:
				{
					sound = "BLOCK_BELL_HIT";

					return true;
				}

				case 259:
				{
					sound = "BLOCK_CAMPFIRE_CRACKLE";

					return true;
				}

				case 260:
				{
					sound = "ROAR";

					return true;
				}

				case 261:
				{
					sound = "STUN";

					return true;
				}

				case 262:
				{
					sound = "BLOCK_SWEET_BERRY_BUSH_HURT";

					return true;
				}

				case 263:
				{
					sound = "BLOCK_SWEET_BERRY_BUSH_PICK";

					return true;
				}

				case 264:
				{
					sound = "BLOCK_CARTOGRAPHY_TABLE_USE";

					return true;
				}

				case 265:
				{
					sound = "BLOCK_STONECUTTER_USE";

					return true;
				}

				case 266:
				{
					sound = "BLOCK_COMPOSTER_EMPTY";

					return true;
				}

				case 267:
				{
					sound = "BLOCK_COMPOSTER_FILL";

					return true;
				}

				case 268:
				{
					sound = "BLOCK_COMPOSTER_FILL_SUCCESS";

					return true;
				}

				case 269:
				{
					sound = "BLOCK_COMPOSTER_READY";

					return true;
				}

				case 270:
				{
					sound = "BLOCK_BARREL_OPEN";

					return true;
				}

				case 271:
				{
					sound = "BLOCK_BARREL_CLOSE";

					return true;
				}

				case 272:
				{
					sound = "RAID_HORN";

					return true;
				}

				case 273:
				{
					sound = "BLOCK_LOOM_USE";

					return true;
				}

				case 274:
				{
					sound = "AMBIENT_IN_RAID";

					return true;
				}

				case 275:
				{
					sound = "UI_CARTOGRAPHY_TABLE_TAKE_RESULT";

					return true;
				}

				case 276:
				{
					sound = "UI_STONECUTTER_TAKE_RESULT";

					return true;
				}

				case 277:
				{
					sound = "UI_LOOM_TAKE_RESULT";

					return true;
				}

				case 278:
				{
					sound = "BLOCK_SMOKER_SMOKE";

					return true;
				}

				case 279:
				{
					sound = "BLOCK_BLASTFURNACE_FIRE_CRACKLE";

					return true;
				}

				case 280:
				{
					sound = "BLOCK_SMITHING_TABLE_USE";

					return true;
				}

				case 281:
				{
					sound = "SCREECH";

					return true;
				}

				case 282:
				{
					sound = "SLEEP";

					return true;
				}

				case 283:
				{
					sound = "BLOCK_FURNACE_LIT";

					return true;
				}

				case 284:
				{
					sound = "CONVERT_MOOSHROOM";

					return true;
				}

				case 285:
				{
					sound = "MILK_SUSPICIOUSLY";

					return true;
				}

				case 286:
				{
					sound = "CELEBRATE";

					return true;
				}

				case 287:
				{
					sound = "JUMP_PREVENT";

					return true;
				}

				case 288:
				{
					sound = "AMBIENT_POLLINATE";

					return true;
				}

				case 289:
				{
					sound = "BLOCK_BEEHIVE_DRIP";

					return true;
				}

				case 290:
				{
					sound = "BLOCK_BEEHIVE_ENTER";

					return true;
				}

				case 291:
				{
					sound = "BLOCK_BEEHIVE_EXIT";

					return true;
				}

				case 292:
				{
					sound = "BLOCK_BEEHIVE_WORK";

					return true;
				}

				case 293:
				{
					sound = "BLOCK_BEEHIVE_SHEAR";

					return true;
				}

				case 294:
				{
					sound = "DRINK_HONEY";

					return true;
				}

				case 295:
				{
					sound = "AMBIENT_CAVE";

					return true;
				}

				case 296:
				{
					sound = "RETREAT";

					return true;
				}

				case 297:
				{
					sound = "CONVERTED_TO_ZOMBIFIED";

					return true;
				}

				case 298:
				{
					sound = "ADMIRE";

					return true;
				}

				case 299:
				{
					sound = "STEP_LAVA";

					return true;
				}

				case 300:
				{
					sound = "TEMPT";

					return true;
				}

				case 301:
				{
					sound = "PANIC";

					return true;
				}

				case 302:
				{
					sound = "ANGRY";

					return true;
				}

				case 303:
				{
					sound = "AMBIENT_WARPED_FOREST_MOOD";

					return true;
				}

				case 304:
				{
					sound = "AMBIENT_SOULSAND_VALLEY_MOOD";

					return true;
				}

				case 305:
				{
					sound = "AMBIENT_NETHER_WASTES_MOOD";

					return true;
				}

				case 306:
				{
					sound = "RESPAWN_ANCHOR_BASALT_DELTAS_MOOD";

					return true;
				}

				case 307:
				{
					sound = "AMBIENT_CRIMSON_FOREST_MOOD";

					return true;
				}

				case 308:
				{
					sound = "RESPAWN_ANCHOR_CHARGE";

					return true;
				}

				case 309:
				{
					sound = "RESPAWN_ANCHOR_DEPLETE";

					return true;
				}

				case 310:
				{
					sound = "RESPAWN_ANCHOR_SET_SPAWN";

					return true;
				}

				case 311:
				{
					sound = "RESPAWN_ANCHOR_AMBIENT";

					return true;
				}

				case 312:
				{
					sound = "PARTICLE_SOUL_ESCAPE_QUIET";

					return true;
				}

				case 313:
				{
					sound = "PARTICLE_SOUL_ESCAPE_LOUD";

					return true;
				}

				case 314:
				{
					sound = "RECORD_PIGSTEP";

					return true;
				}

				case 315:
				{
					sound = "LODESTONE_COMPASS_LINK_COMPASS_TO_LODESTONE";

					return true;
				}

				case 316:
				{
					sound = "SMITHING_TABLE_USE";

					return true;
				}

				case 317:
				{
					sound = "ARMOR_EQUIP_NETHERITE";

					return true;
				}

				case 318:
				{
					sound = "AMBIENT_WARPED_FOREST_LOOP";

					return true;
				}

				case 319:
				{
					sound = "AMBIENT_SOULSAND_VALLEY_LOOP";

					return true;
				}

				case 320:
				{
					sound = "AMBIENT_NETHER_WASTES_LOOP";

					return true;
				}

				case 321:
				{
					sound = "AMBIENT_BASALT_DELTAS_LOOP";

					return true;
				}

				case 322:
				{
					sound = "AMBIENT_CRIMSON_FOREST_LOOP";

					return true;
				}

				case 323:
				{
					sound = "AMBIENT_WARPED_FOREST_ADDITIONS";

					return true;
				}

				case 324:
				{
					sound = "AMBIENT_SOULSAND_VALLEY_ADDITIONS";

					return true;
				}

				case 325:
				{
					sound = "AMBIENT_NETHER_WASTES_ADDITIONS";

					return true;
				}

				case 326:
				{
					sound = "AMBIENT_BASALT_DELTAS_ADDITIONS";

					return true;
				}

				case 327:
				{
					sound = "AMBIENT_CRIMSON_FOREST_ADDITIONS";

					return true;
				}

				case 328:
				{
					sound = "BUCKET_FILL_POWDER_SNOW";

					return true;
				}

				case 329:
				{
					sound = "BUCKET_EMPTY_POWDER_SNOW";

					return true;
				}

				case 330:
				{
					sound = "UNDEFINED";

					return true;
				}
			}

			sound = null;
			return false;
		}
		#endregion
		
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
					titleComponent.Hide();
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
				else if (entry is ScoreEntryChangeEntity ent)
				{
					if (Client.World.EntityManager.TryGet(ent.EntityId, out var entity))
					{
						if (scoreboard.TryGetObjective(ent.ObjectiveName, out var objective))
						{
							objective.AddOrUpdate(
								ent.Id.ToString(),
								new ScoreboardEntry(ent.Id.ToString(), ent.Score, entity.NameTag));
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
				else
				{
					
				}
			}
		}

		public void HandleMcpeLabTable(McpeLabTable message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
		{
			var converted = ChunkProcessor.GetBlockState(message.blockRuntimeId);

			if (converted != null)
			{
				BlockUpdatePriority priority = (BlockUpdatePriority) message.blockPriority;

				Client.World?.SetBlockState(
					new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z), 
					converted, (int) message.dataLayerId, priority);
			}
			else
			{
				Log.Warn($"Received unknown block runtime id.");
			}
			//UnhandledPackage(message);
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

		private void HandleMcpeEmote(McpeEmote message)
		{
			/*if (Client.World.EntityManager.TryGet(message.RuntimeEntityId, out var entity))
			{
				if (entity.AnimationController.)
			}*/
			UnhandledPackage(message);
		}

		private void HandleMcpeAnimateEntity(McpeAnimateEntity message)
		{
			/*if (Client.World.EntityManager.TryGet(message.Controller, out var entity))
			{
				if (entity.AnimationController.)
			}*/
			UnhandledPackage(message);
		}
		
		public bool HandleOtherPackets(Packet message)
		{
			switch (message)
			{
				case McpeEmote emote:
				{
					HandleMcpeEmote(emote); 
				} return true;
				
				case McpeAnimateEntity emote:
				{
					HandleMcpeAnimateEntity(emote); 
				} return true;
			}
			
			UnhandledPackage(message);
			return false;
		}
		
		/// <inheritdoc />
		public void ReportPackets()
		{
			var entries = UnhandledPackets.ToArray();
			UnhandledPackets.Clear();
			
			foreach (var p in entries)
			{
				var type = p.Value.type;
				if (type == null)
					continue;
				
				Log.Warn(
					$"Unhandled packet: {type.FullName} * {p.Value.count} (0x{p.Key:x2})");
			}
		}
	}
}
