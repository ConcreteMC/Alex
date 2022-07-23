using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using Alex.Audio;
using Alex.Blocks;
using Alex.Common.Resources;
using Alex.Common.Services;
using Alex.Common.Utils;
using Alex.Common.World;
using Alex.Entities;
using Alex.Entities.BlockEntities;
using Alex.Entities.Components.Effects;
using Alex.Entities.Projectiles;
using Alex.Gui.Dialogs;
using Alex.Gui.Elements.Scoreboard;
using Alex.Interfaces;
using Alex.Interfaces.Resources;
using Alex.Net.Bedrock.Packets;
using Alex.Networking.Java.Packets.Play;
using Alex.ResourcePackLib.Json.Bedrock.Sound;
using Alex.Services;
using Alex.Utils;
using Alex.Utils.Commands;
using Alex.Utils.Inventories;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Multiplayer.Bedrock;
using Alex.Worlds.Multiplayer.Bedrock.Resources;
using fNbt;
using Jose;
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
using Color = Microsoft.Xna.Framework.Color;
using CommandProperty = Alex.Utils.Commands.CommandProperty;
using Dimension = Alex.Worlds.Dimension;
using Entity = Alex.Entities.Entity;
using GameMode = MiNET.Worlds.GameMode;
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

		private WorldProvider WorldProvider { get; }
		private PlayerProfile PlayerProfile { get; }

		private BedrockTransactionTracker TransactionTracker => Client.TransactionTracker;

		public BedrockClientPacketHandler(BedrockClient client,
			WorldProvider worldProvider,
			PlayerProfile profile,
			Alex alex,
			CancellationToken cancellationToken,
			ChunkProcessor chunkProcessor)
		{
			Client = client;
			AlexInstance = alex;
			CancellationToken = cancellationToken;
			WorldProvider = worldProvider;
			PlayerProfile = profile;

			AnvilWorldProvider.LoadBlockConverter();

			ChunkProcessor = chunkProcessor;
		}

		public bool ReportUnhandled { get; set; } = true;

		private Dictionary<int, (Type type, ulong count)> UnhandledPackets =
			new Dictionary<int, (Type type, ulong count)>();

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
				var data = JWT.Payload<HandshakeData>(token);
				Client.InitiateEncryption(Base64Url.Decode(x5u), Base64Url.Decode(data.salt.TrimEnd('=')));
			}
			catch (Exception e)
			{
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

		public void HandleMcpePlayStatus(McpePlayStatus message)
		{
			var playStatus = (McpePlayStatus.PlayStatus)message.status;
			Client.PlayerStatus = playStatus;

			if (playStatus == McpePlayStatus.PlayStatus.LoginSuccess)
			{
				Client.ResourcePackManager.StatusChanged += ResourcePackStatusChanged;
				Log.Info(
					$"Play Status: Login success, reporting cache status as {(ChunkProcessor.Cache.Enabled ? "Enabled" : "Disabled")}");

				McpeClientCacheStatus status = McpeClientCacheStatus.CreateObject();
				status.enabled = ChunkProcessor.Cache.Enabled;
				Client.SendPacket(status);
			}
			else if (playStatus == McpePlayStatus.PlayStatus.LoginFailedClient)
			{
				Log.Info($"Client out of date!");

				Client.ShowDisconnect(
					"multiplayer.status.client_out_of_date", true, true, DisconnectReason.ClientOutOfDate);
			}
			else if (playStatus == McpePlayStatus.PlayStatus.LoginFailedServer)
			{
				Log.Info($"Server out of date!");

				Client.ShowDisconnect(
					"multiplayer.status.server_out_of_date", true, true, DisconnectReason.ServerOutOfDate);
			}
			else if (playStatus == McpePlayStatus.PlayStatus.PlayerSpawn)
			{
				Log.Info($"Play Status: Player spawn");
				Client.MarkAsInitialized();
			}
			else
			{
				Log.Warn($"Received unknown Play Status: ({message.status}){playStatus}");
				Client.ShowDisconnect($"Unrecognized play status.", false, true, DisconnectReason.Network);
			}
		}

		private void ResourcePackStatusChanged(object sender, ResourcePackManager.ResourceStatusChangedEventArgs e)
		{
			if (e.Status == ResourcePackManager.ResourceManagerStatus.Ready)
			{
			//	Client.RequestRenderDistance(0, Client.World.ChunkManager.RenderDistance);
			}
		}

		public void HandleMcpeDisconnect(McpeDisconnect message)
		{
			Log.Info($"Received disconnect: {message.message}");
			Client.ShowDisconnect(message.message, false, true, DisconnectReason.Kicked);
		}

		public void HandleMcpeResourcePackDataInfo(McpeResourcePackDataInfo message)
		{
			Client.ResourcePackManager.HandleMcpeResourcePackDataInfo(message);
		}

		public void HandleMcpeResourcePackChunkData(McpeResourcePackChunkData message)
		{
			Client.ResourcePackManager.HandleMcpeResourcePackChunkData(message);
		}

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
					rawMessage =
						$"{ChatFormatting.Italic}{message.source}{ChatFormatting.Reset}{ChatFormatting.Italic} whispered{ChatFormatting.Reset}: {rawMessage}";

					break;

				case 8: //Announcement
					rawMessage = $"[{message.source}{ChatFormatting.Reset}]: {rawMessage}";

					break;
			}

			WorldProvider?.ChatRecipient?.AddMessage(rawMessage, (MessageType)message.type);
		}

		public void HandleMcpeSetTime(McpeSetTime message)
		{
			Client.World?.SetTime(message.time, message.time % 24000);

			Client.ChangeDimensionResetEvent.Set();
		}
		
		public void HandleMcpeStartGame(McpeStartGame message)
		{
			Log.Warn($"Experimental gameplay override={message.levelSettings.experimentalGameplayOverride} Experiments={message.levelSettings.experiments.Count} BaseGameVersion={message.levelSettings.gameVersion}");

			if ( message.levelSettings.gameVersion.Trim().StartsWith("1.17"))
			{
				ChunkProcessor.WorldSettings = new WorldSettings(256, 0);
			}

			foreach (var experiment in message.levelSettings.experiments)
			{
				Log.Info($"Experiment: name={experiment.Name} enabled={experiment.Enabled}");
			}
			
			var player = Client.World.Player;

			if (message.movementType > 0)
			{
				Client.ServerAuthoritiveMovement = true;
				Log.Warn($"!!! Server uses server-authoritive movement, this is not yet FULLY supported!");
			}

			try
			{
				player.EntityId = Client.EntityId = message.runtimeEntityId;
				Client.NetworkEntityId = message.entityIdSelf;
				Client.World.SetRain(false, message.levelSettings.rainLevel);
				Client.World.SetThunder(false, message.levelSettings.lightningLevel);
				Client.Tick = message.currentTick;

				//Client.World.SpawnPoint 
				//	Client.SpawnPoint = new Vector3(
				//		message.spawn.X, message.spawn.Y - Player.EyeLevel, message.spawn.Z); //message.spawn;

				Client.SetDimension((Dimension)message.levelSettings.spawnSettings.Dimension);
				//Client.World.Dimension = (Dimension)message.levelSettings.spawnSettings.Dimension;
				Client.World.Difficulty = (Difficulty)message.levelSettings.difficulty;

				Client.World?.UpdatePlayerPosition(
					new PlayerLocation(
						new Microsoft.Xna.Framework.Vector3(message.spawn.X, message.spawn.Y, message.spawn.Z),
						message.rotation.Y, message.rotation.Y, message.rotation.X), true);

				if (message.enableNewInventorySystem)
				{
					Client.ServerAuthoritiveInventory = true;
					Log.Info($"Using server authoritive inventories.");
				}

				if (message.enableNewBlockBreakSystem)
				{
					Log.Info($"New blockbreak system?");
				}

				player.SetInventory(new BedrockInventory(46));
				player.UpdateGamemode((Interfaces.GameMode)message.playerGamemode);
				player.PermissionLevel = (PermissionLevel)message.levelSettings.permissionLevel;

				player.Controller.SetRewindHistorySize(
					message.movementRewindHistorySize); //.RewindHistory = message.movementRewindHistorySize;

				foreach (var gr in message.levelSettings.gamerules)
				{
					Client.World.SetGameRule(gr);
				}

				ChunkProcessor.Itemstates = message.itemstates;
				Log.Info($"Start game, spawn: {message.spawn}");
			}
			finally
			{
				Client.GameStarted = true;
				Client.RequestRenderDistance(0, Client.World.ChunkManager.RenderDistance);

				//Client.RequestChunkRadius(AlexInstance.Options.AlexOptions.VideoOptions.RenderDistance.Value);
			}
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			var pos = new PlayerLocation(
				message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch)
			{
				OnGround = message.onGround
			};

			Client.World.UpdateEntityPosition(
				message.runtimeEntityId, pos, false, true, true, adjustForEntityHeight: true,
				teleport: message.mode == 2);
		}

		private void UpdateEntityAdventureFlags(Entity entity,
			uint flags,
			uint actionPermissions,
			uint commandPermissions,
			uint permissionLevel,
			uint customStoredPermissions)
		{
			if (entity == null)
				return;

			entity.IsWorldImmutable = (flags & 0x01) != 0;
			entity.IsNoPvP = (flags & 0x02) != 0;
			entity.IsNoPvM = (flags & 0x04) != 0;
			entity.CanFly = (flags & 0x40) != 0;

			//if (entity is Player)
			entity.HasCollision = (flags & 0x80) == 0;
			entity.IsFlying = (flags & 0x200) != 0;

			if (entity is RemotePlayer player)
			{
				player.ActionPermissions = (ActionPermissions)actionPermissions;
				player.CommandPermissions = (CommandPermission)commandPermissions;
				player.PermissionLevel = (PermissionLevel)permissionLevel;
				player.CustomStoredPermissions = customStoredPermissions;
			}
		}

		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			var userId = BinaryPrimitives.ReverseEndianness(message.entityUniqueId);
			Entity entity = null;

			if (userId == Client.World.Player.EntityId)
			{
				entity = Client.World.Player;
			}
			else if (!Client.World.TryGetEntity(userId, out entity)) { }

			if (entity == null)
				return;

			UpdateEntityAdventureFlags(
				entity, message.flags, message.actionPermissions, message.commandPermission, message.permissionLevel,
				message.customStoredPermissions);
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
					message.x, message.y, message.z, -message.headYaw, -message.yaw,
					-message.pitch) { OnGround = true };

				remotePlayer.Velocity = new Microsoft.Xna.Framework.Vector3(
					message.speedX, message.speedY, message.speedZ);

				if (message.item != null)
				{
					remotePlayer.Inventory.MainHand = message.item.ToAlexItem();
				}

				if (message.metadata != null)
				{
					remotePlayer.HandleMetadata(message.metadata);
				}

				HandleAbilityLayers(remotePlayer, message.layers);
				//TODO: Fix adventure flags
				//UpdateEntityAdventureFlags(
				//	remotePlayer, message.flags, message.actionPermissions, message.commandPermission,
				//	message.permissionLevel, message.customStoredPermissions);

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
							new PlayerListItem(r.ClientUuid, r.DisplayName, (GameMode)((int)r.GameMode), -1));
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
			public MiNET.Utils.Skins.Skin Skin { get; set; }
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
			Entity entity = EntityFactory.Create(message.entityType, Client.World);

			if (entity == null)
			{
				Log.Warn($"Cannot spawn entity of type: {message.entityType}");

				return;
			}

			entity.RenderLocation = entity.KnownPosition = new PlayerLocation(
				message.x, message.y, message.z, -message.headYaw, -message.yaw, -message.pitch);

			entity.KnownPosition.OnGround = false;

			entity.Velocity = new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ);
			entity.EntityId = message.runtimeEntityId;
			entity.UUID = new UUID(Guid.NewGuid().ToByteArray());

			entity.SetInventory(new BedrockInventory(46));


			if (message.metadata != null)
				entity.HandleMetadata(message.metadata);

			if (message.attributes != null)
				entity.UpdateAttributes(message.attributes);

			if (message.links != null)
			{
				//	foreach (var link in message.links)
				{ }
			}

			if (!Client.World.SpawnEntity(entity))
			{
				Log.Warn(
					$"Could not spawn in entity, an entity with this runtimeEntityId already exists! (Runtime: {message.runtimeEntityId} Self: {message.entityIdSelf})");
			}
		}

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

			if (!Client.World.SpawnEntity(itemEntity))
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
			if (message.runtimeEntityId == Client.EntityId) return;

			//	if (message is EntityDelta ed)
			{
				if (Client.World.TryGetEntity(message.runtimeEntityId, out var entity))
				{
					var pos = new MiNET.Utils.Vectors.PlayerLocation(
						entity.KnownPosition.X, entity.KnownPosition.Y, entity.KnownPosition.Z,
						entity.KnownPosition.HeadYaw, entity.KnownPosition.Yaw, entity.KnownPosition.Pitch);

					var known = message.GetCurrentPosition(pos);

					var newPosition = new PlayerLocation(
						known.X, known.Y, known.Z, known.HeadYaw, known.Yaw, known.Pitch);

					if ((message.flags & McpeMoveEntityDelta.HasY) != 0)
					{
						if (entity is RemotePlayer player)
						{
							newPosition.Y -= Player.EyeLevel;
						}
					}

					Client.World.UpdateEntityPosition(
						message.runtimeEntityId, newPosition, false, true, true, false, false);
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
				BlockUpdatePriority priority = (BlockUpdatePriority)message.blockPriority;

				Client.World?.SetBlockState(
					new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z),
					converted, (int)message.storage, priority);
			}
			else
			{
				//Log.Warn($"Received unknown block runtime id.");
			}
		}

		public void HandleMcpeAddPainting(McpeAddPainting message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeTickSync(McpeTickSync message)
		{
			Log.Info($"Received TickSync! RequestTime={message.requestTime} ResponseTime={message.responseTime} Delay={(message.responseTime - message.requestTime)}");
			UnhandledPackage(message);
		}

		public void HandleMcpeSpawnParticleEffect(McpeSpawnParticleEffect message)
		{
			if (!AlexInstance.ParticleManager.SpawnParticle(
				    message.particleName,
				    new Microsoft.Xna.Framework.Vector3(message.position.X, message.position.Y, message.position.Z)))
			{
				Log.Warn($"Unknown particle: {message.particleName}");
			}
		}

		private ConcurrentDictionary<string, int> _entityIdentifiers = new ConcurrentDictionary<string, int>();

		public void HandleMcpeAvailableEntityIdentifiers(McpeAvailableEntityIdentifiers message)
		{
			var idList = message.namedtag.NbtFile?.RootTag?["idlist"];

			if (idList != null && idList is NbtList ids)
			{
				foreach (NbtCompound tag in ids.ToArray<NbtCompound>())
				{
					if (tag.TryGet("id", out NbtString id) && tag.TryGet("rid", out var realId))
					{
						int identifier = 0;

						if (realId is NbtInt nbtInt)
							identifier = nbtInt.Value;

						if (realId is NbtByte nbtByte)
							identifier = nbtByte.Value;
						
						_entityIdentifiers[id.Value] = identifier;

						if (LoggingConstants.LogServerEntityDefinitions)
#pragma warning disable CS0162
							Log.Debug($"Registered entity identifier: {id.Value}");
#pragma warning restore CS0162
					}
				}
			}
		}

		public void HandleMcpeNetworkSettings(McpeNetworkSettings message)
		{
			var threshold = BitConverter.ToInt16(new byte[] { message.unknown, message.compressionThreshold });

			Client.Connection.Session.CompressionThreshold = threshold;

			Log.Info($"Compression Threshold: {threshold}");
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
		}

		/// <inheritdoc />
		public void HandleMcpeItemComponent(McpeItemComponent message)
		{
			//	foreach (var entry in message.entries)
			//	{
			//		if (entry.Nbt != null)
			//			Log.Info($"ItemComponent! Name={entry.Name} Nbt={entry.Nbt.NbtFile.ToString()}");
			//	}
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeFilterTextPacket(McpeFilterTextPacket message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeUpdateSubChunkBlocksPacket(McpeUpdateSubChunkBlocksPacket message)
		{
			UnhandledPackage(message);
		}

		/// <inheritdoc />
		public void HandleMcpeDimensionData(McpeDimensionData message)
		{
			Client.DimensionDefinitions = message.definitions;
		}

		/// <inheritdoc />
		public void HandleMcpeUpdateAbilities(McpeUpdateAbilities message)
		{
			var userId = BinaryPrimitives.ReverseEndianness(message.entityUniqueId);
			var player = Client.World.Player;
			if (message.entityUniqueId == 0 || message.entityUniqueId == player.EntityId || userId == player.EntityId)
			{
				HandleAbilityLayers(player, message.layers);
			}
			//TODO: Implement update abilities
			UnhandledPackage(message);
		}

		private void HandleAbilityLayers(Entity entity, AbilityLayers layers)
		{
			foreach (var layer in layers)
			{
				entity.SetAbilityLayer(layer);
			}
		}

		/// <inheritdoc />
		public void HandleMcpeUpdateAdventureSettings(McpeUpdateAdventureSettings message)
		{
			var player = Client.World.Player;
			player.IsNoPvM = message.noPvm;
			player.IsWorldImmutable = message.immutableWorld;
		}

		/// <inheritdoc />
		public void HandleMcpeAlexEntityAnimation(McpeAlexEntityAnimation message) { }

		public void HandleMcpeNetworkChunkPublisherUpdate(McpeNetworkChunkPublisherUpdate message)
		{
			Client.ChunkPublisherPosition = new BlockCoordinates(
				message.coordinates.X, message.coordinates.Y, message.coordinates.Z);

			Client.ChunkPublisherRadius = message.radius;
				//Log.Info($"ChunkPublisherUpdate: radius={message.radius} coordinates={message.coordinates}");
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
			ChunkProcessor.HandleClientCacheMissResponse(message);
		}

		public void HandleMcpeLevelEvent(McpeLevelEvent message)
		{
			var msgId = message.eventId;

			if (msgId >= 0x4000 && msgId <= (0x4000 | (int)ParticleType.Sneeze)) //Got particle :)
			{
				var particleType = ((ParticleType)msgId - 0x4000);

				if (!AlexInstance.ParticleManager.SpawnParticle(
					    particleType,
					    new Microsoft.Xna.Framework.Vector3(message.position.X, message.position.Y, message.position.Z),
					    message.data))
				{
					if (Client.IsConnected && LoggingConstants.LogUnknownParticles)
						Log.Warn($"Unknown particle type: {particleType}");
				}

				return;
			}

			var blockcoords = (BlockCoordinates)new Microsoft.Xna.Framework.Vector3(
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
					var ticksRequired = (double)ushort.MaxValue / message.data;

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
			var worldPos = new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z);

			if (Client.World.EntityManager.TryGetBlockEntity(worldPos, out var blockEntity))
			{
				blockEntity.HandleBlockAction((byte)message.case1, message.case2);
			}
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			if (Client.World.TryGetEntity(message.runtimeEntityId, out Entity entity))
			{
				entity.HandleEntityEvent(message.eventId, message.data);
			}
		}

		public void HandleMcpeMobEffect(McpeMobEffect message)
		{
			Entity entity;

			if (Client.World.EntityManager.TryGet(message.runtimeEntityId, out entity))
			{
				Effect effect = null;
				MiNET.Effects.EffectType effectType = (MiNET.Effects.EffectType)message.effectId;
				EffectType alexEffectType = (EffectType)message.effectId;

				switch (effectType)
				{
					case MiNET.Effects.EffectType.Slowness:
						alexEffectType = EffectType.Slowness;
						effect = new SlownessEffect();

						break;

					case MiNET.Effects.EffectType.Speed:
						alexEffectType = EffectType.Speed;
						effect = new SpeedEffect();

						break;

					case MiNET.Effects.EffectType.Haste:
						alexEffectType = EffectType.Haste;
						effect = new HasteEffect();

						break;

					case MiNET.Effects.EffectType.MiningFatigue:
						alexEffectType = EffectType.MiningFatigue;
						effect = new MiningFatigueEffect();

						break;

					case MiNET.Effects.EffectType.JumpBoost:
						alexEffectType = EffectType.JumpBoost;
						effect = new JumpBoostEffect();

						break;

					case MiNET.Effects.EffectType.NightVision:
						alexEffectType = EffectType.NightVision;
						effect = new NightVisionEffect();

						break;

					default:
						Log.Warn(
							$"Missing effect implementation. Alex={(EffectType)message.effectId} MiNET={(MiNET.Effects.EffectType)message.effectId}");

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
						if (entity.Effects.TryGetEffect(alexEffectType, out effect))
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
			Log.Warn(
				$"Inventory Transaction. TransactionRecords={message.transaction?.TransactionRecords?.Count ?? 0} RequestRecords={message.transaction?.RequestRecords?.Count ?? 0} HasNetworkIds={message.transaction?.HasNetworkIds ?? false}");

			UnhandledPackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			if (Client.EntityId == message.runtimeEntityId && message.windowsId == 0)
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
					entity.Inventory.SelectedSlot = message.selectedSlot;
				}
				else
				{
					Log.Warn(
						$"Inventory slot {slot} is out of range for entity: {entity.ToString()} (Min=0 Max={entity.Inventory.SlotCount})");
				}
			}
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
			}

			if (entity == null)
			{
				//Log.Warn($"Unknown entity in MobArmorEquipment packet! ({message.runtimeEntityId})");

				return;
			}

			World.BackgroundWorker.Enqueue(
				() =>
				{
					entity.Inventory.Helmet = message.helmet.ToAlexItem().Clone();
					entity.Inventory.Chestplate = message.chestplate.ToAlexItem().Clone();
					entity.Inventory.Leggings = message.leggings.ToAlexItem().Clone();
					entity.Inventory.Boots = message.boots.ToAlexItem().Clone();
				});
		}

		public void HandleMcpeInteract(McpeInteract message)
		{
			Log.Info($"Received McpeInteract. ActionID={message.actionId} EntityId={message.targetRuntimeEntityId}");
			UnhandledPackage(message);
		}

		public void HandleMcpeHurtArmor(McpeHurtArmor message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetEntityData(McpeSetEntityData message)
		{
			Entity entity = null;

			if (Client.EntityId == message.runtimeEntityId || message.runtimeEntityId == Client.World.Player.EntityId)
			{
				entity = Client.World.Player;
			}
			else if (Client.World.TryGetEntity(message.runtimeEntityId, out var e))
			{
				entity = e;
			}

			if (entity == null)
			{
				//	Log.Warn($"Unknown entity in SetEntityData: {message.runtimeEntityId}");
				return;
			}

			entity.HandleMetadata(message.metadata);
		}

		public void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			var v = message.velocity;
			var velocity = new Microsoft.Xna.Framework.Vector3(v.X, v.Y, v.Z);

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

			entity.Movement.Velocity(velocity);
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
			if (message.spawnType == 1) //Compass position
			{
				Client.World.CompassPosition = new BlockCoordinates(
					message.coordinates.X, (message.coordinates.Y), message.coordinates.Z);
			}
			else if (message.spawnType == 0) //Player spawn
			{
				//Client.World.Player.SpawnPoint = new BlockCoordinates(
				//		message.coordinates.X, (message.coordinates.Y), message.coordinates.Z);
			}

			Client.World.SpawnPoint = new Microsoft.Xna.Framework.Vector3(
				message.unknownCoordinates.X, (float)(message.unknownCoordinates.Y), message.unknownCoordinates.Z);
			//Client.World.Dimension = (Dimension)message.dimension;
			//Client.World.SpawnPoint = new Microsoft.Xna.Framework.Vector3(message.coordinates.X, (float) (message.coordinates.Y), message.coordinates.Z);

			Log.Debug($"SetSpawn! (Position={message.coordinates}) (SpawnPosition={message.unknownCoordinates})");
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
			Log.Info(
				$"Respawn state {(McpeRespawn.RespawnState)message.state} | Runtime entity id: {message.runtimeEntityId}");

			switch ((McpeRespawn.RespawnState)message.state)
			{
				case McpeRespawn.RespawnState.Search:
				{
					var response = McpeRespawn.CreateObject();
					response.runtimeEntityId = message.runtimeEntityId;
					response.x = message.x;
					response.y = message.y;
					response.z = message.z;
					response.state = (int)McpeRespawn.RespawnState.ClientReady;

					Client.SendPacket(response);
				}

					break;

				case McpeRespawn.RespawnState.Ready:
				{
					Client.World.SpawnPoint = new Microsoft.Xna.Framework.Vector3(
						message.x, (float)(message.y), message.z);

					Client.World.UpdatePlayerPosition(new PlayerLocation(message.x, message.y, message.z), true);

					if (Client.CanSpawn)
					{
						Client.SendMcpeMovePlayer(new PlayerLocation(message.x, message.y, message.z), 1);
					}

					Client.RequestRenderDistance(0, Client.World.ChunkManager.RenderDistance);

					Client.ChangeDimensionResetEvent.Set();

					Client.SendPlayerAction(PlayerAction.Respawn, null, null);
				}

					break;

				case McpeRespawn.RespawnState.ClientReady: { }

					break;
			}
		}

		public void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			try
			{
				var windowId = message.windowId;

				int data = -1;
				if (Client.World.EntityManager.TryGetBlockEntity(
					    new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z),
					    out var blockEntity))
				{
					if (blockEntity is ChestBlockEntity chestBlockEntity)
					{
						data = (chestBlockEntity.ChestType & ChestType.Single) != 0 ? 27 : 54;
					}
				}
				
				var dialog = Client.World.InventoryManager.Show(
					Client.World.Player.Inventory, message.windowId, (ContainerType)message.type, data);

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

			if (Client.World.InventoryManager.TryGet((int)message.inventoryId, out var container))
			{
				for (var index = 0; index < message.input.Count; index++)
				{
					var slot = message.input[index];

					var result = slot.ToAlexItem().Clone();

					if (result != null)
					{
						result.StackID = slot.UniqueId;
						container.Inventory.SetSlot(startIndex + index, result, true);
						//inventory[usedIndex] = result;
					}
					else
					{
						Log.Warn(
							$"Failed to set window slot: {index} Id: {slot.Id}:{slot.Metadata} (Window={message.inventoryId})");
					}
				}

				return;
			}

			InventoryBase inventory = null;
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
						Log.Warn(
							$"Slot {adjustedIndex} out of range for inventory {message.inventoryId} (Limit: {inventory.SlotCount})");
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

			if (Client.World.InventoryManager.TryGet((int)message.inventoryId, out var container))
			{
				var slotIndex = (int)message.slot;
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
				Log.Warn(
					$"(McpeInventorySlot) Unknown inventory ID: {message.inventoryId} ({JsonConvert.SerializeObject(message.item)})");

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

				BlockEntityFactory.ReadFrom(compound, Client.World, worldPos);
			}
			else
			{
				Log.Warn($"Invalid roottag for BlockEntityData. Got: {message.namedtag.NbtFile.RootTag.TagType}");
			}
		}

		public void HandleMcpeLevelChunk(McpeLevelChunk msg)
		{
			var blobs = msg.blobHashes;
			var cacheEnabled = msg.cacheEnabled;
			var requestMode = msg.subChunkRequestMode;
			var subChunkCount = msg.subChunkCount;
			var chunkData = msg.chunkData;
			var cx = msg.chunkX;
			var cz = msg.chunkZ;
			msg.PutPool();

			//if (chunkData[0] < 1)
			if (subChunkCount < 1)
			{
				//Nothing to read.
				return;
			}

			ChunkProcessor.HandleChunkData(cacheEnabled, requestMode, blobs, subChunkCount, chunkData, cx, cz);
		}
		
		/// <inheritdoc />
		public void HandleMcpeSubChunkPacket(McpeSubChunkPacket message)
		{
			var baseCoordinates = message.subchunkCoordinates;

			foreach (var entry in message.entries)
			{
				var pos = new BlockCoordinates(
					baseCoordinates.X + entry.Offset.XOffset, baseCoordinates.Y + entry.Offset.YOffset,
					baseCoordinates.Z + entry.Offset.ZOffset);

				var result = entry.RequestResult;
				
				if (entry is SubChunkEntryWithCache entryWithCache)
				{
					ChunkProcessor.HandleSubChunkData(result, (ulong) entryWithCache.usedBlobHash, entry.Data, pos.X, pos.Y, pos.Z);
				}
				else if (entry is SubChunkEntryWithoutCache entryWithoutCache)
				{
					ChunkProcessor.HandleSubChunkData(result, null, entry.Data, pos.X, pos.Y, pos.Z);
				}
				else
				{
					Log.Warn($"Unknwon subchunk type. Result={result}");
				}
			}
			
			message?.PutPool();
			//UnhandledPackage(message);
		}

		//private object _changeDimensionLock = new object();
		private SemaphoreSlim _changeDimensionSemaphone = new SemaphoreSlim(1);

		public void HandleMcpeChangeDimension(McpeChangeDimension message)
		{
			Log.Info(
				$"Initiating dimension change! Dimension={message.dimension} Respawn={message.respawn} Position={message.position}");
			
			World world = Client.World;
			Client.SetDimension((Dimension) message.dimension);

			Client.ResetInitialized();
			var pos = new PlayerLocation(message.position.X, message.position.Y, message.position.Z);
			Client.ChunkPublisherPosition = new BlockCoordinates(pos);

			world.UpdatePlayerPosition(pos, true);

			WorldProvider?.BossBarContainer?.Reset();
			_bossBarMapping.Clear();

			McpePlayerAction action = McpePlayerAction.CreateObject();
			action.runtimeEntityId = Client.EntityId;
			action.actionId = (int)PlayerAction.DimensionChangeAck;
			Client.SendPacket(action);

			if (!_changeDimensionSemaphone.Wait(10, CancellationToken))
			{
				return;
			}

			if (Client.World.Player.IsSpawned)
			{
				ChunkProcessor.Clear();
				world.ClearChunksAndEntities();
			}

			Client.World.Player.OnDespawn();
			
			//AlexInstance.GuiManager.HideDialog<WorldLoadingDialog>();
			
			bool cancelled = false;
			WorldLoadingDialog worldLoadingDialog = AlexInstance.GuiManager.CreateDialog<WorldLoadingDialog>();
			worldLoadingDialog.ConnectingToServer = true;

			worldLoadingDialog.CancelAction = () =>
			{
				cancelled = true;

				Client.ShowDisconnect("Disconnect requested by user.", false, false, DisconnectReason.Unknown);

				Client.Close();
			};

			worldLoadingDialog.UpdateProgress(LoadingState.LoadingChunks, 0);

			ThreadPool.QueueUserWorkItem(
				(o) =>
				{
					try
					{
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

							double radiusSquared = Math.Pow(Client.World.ChunkManager.RenderDistance, 2);
							var target = radiusSquared;

							percentage = (int)((100 / target) * world.ChunkManager.ChunkCount);

							if (percentage != previousPercentage)
							{
								worldLoadingDialog.UpdateProgress(state, percentage);
								previousPercentage = percentage;
							}

							if (!ready)
							{
								if (Client.ChangeDimensionResetEvent.WaitOne(5))
								{
									ready = true;
								}
							}

							if (Client.CanSpawn && !Client.World.Player.WaitingOnChunk)
							{
								break;
							}
						} while (Client.IsConnected);

						Client.MarkAsInitialized();
						Client.World.Player.OnSpawn();

						Log.Info($"Finished Respawn. Dimension={Client.World.Dimension}");
					}
					finally
					{
						worldLoadingDialog.Close();
						_changeDimensionSemaphone.Release();
					}
				});
		}

		public void HandleMcpeSetCommandsEnabled(McpeSetCommandsEnabled message)
		{
			Client.CommandProvider.Enabled = message.enabled;
			//UnhandledPackage(message);
		}

		public void HandleMcpeSetDifficulty(McpeSetDifficulty message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
		{
			Client.World.Player.UpdateGamemode((Interfaces.GameMode)message.gamemode);
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
		}

		public void HandleMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate message)
		{
			Log.Info(
				$"Received chunkradius. Requested={AlexInstance.Options.AlexOptions.VideoOptions.RenderDistance.Value} Received={message.chunkRadius}");

			Client.World.ChunkManager.RenderDistance = message.chunkRadius;

			if (Client.PlayerStatus != McpePlayStatus.PlayStatus.PlayerSpawn)
			{
				Client.SendTickSync();
			}
			//	Client.ChunkRadius = message.chunkRadius;
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

		private ConcurrentDictionary<long, MiNET.Utils.UUID> _bossBarMapping =
			new ConcurrentDictionary<long, MiNET.Utils.UUID>();

		public void HandleMcpeBossEvent(McpeBossEvent message)
		{
			var container = WorldProvider.BossBarContainer;

			if (container == null)
				return;

			switch ((McpeBossEvent.Type)message.eventType)
			{
				case McpeBossEvent.Type.AddBoss:

					_bossBarMapping.GetOrAdd(
						message.bossEntityId, l =>
						{
							MiNET.Utils.UUID uuid;

							var text = message.title;
							var health = message.healthPercent;

							if (Client.World.TryGetEntity(message.bossEntityId, out var entity))
							{
								if (string.IsNullOrWhiteSpace(text))
									text = entity.NameTag;
							}

							uuid = new MiNET.Utils.UUID(Guid.NewGuid().ToByteArray());

							var customColor = AlexInstance.GuiRenderer.GetGlobalOrDefault(
								"$boss_health_bar_color", Primitives.Factory.Vector3Zero);

							container.Add(
								uuid, text, health, BossBarPacket.BossBarColor.Pink,
								BossBarPacket.BossBarDivisions.None, 0,
								customColor == Primitives.Factory.Vector3Zero ? (Color?)null :
									new Color(new Vector3(customColor.X, customColor.Y, customColor.Z)));

							return uuid;
						});

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

									c.Properties.Add(
										new EnumCommandProperty(param.Name, !param.Optional, options, enumName));
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

							c.Description = c.Describe().FirstOrDefault(version.Description);
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
			ushort port = message.port;

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
						new BedrockServerType(AlexInstance),
						new ServerConnectionDetails(new IPEndPoint(resolved.Result, port)), PlayerProfile);
				});
		}

		public void HandleMcpeLevelSoundEventOld(McpeLevelSoundEventOld message)
		{
			PlaySound(
				message.soundId, message.blockId, message.entityType.ToString(), message.position, message.isGlobal);
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

			if (AudioHelpers.TryConvertSoundIdToMapping(soundId, out soundEvent))
			{
				soundEvent = soundEvent.ToLower();


				string entityType = string.IsNullOrWhiteSpace(entityTypeId) ? null :
					new ResourceLocation(entityTypeId).Path;

				var blockstate = BlockFactory.GetBlockState((uint)blockId);
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
						if (collection.EntitySounds?.Entities != null)
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
					}

					if (!string.IsNullOrWhiteSpace(blockName))
					{
						if (collection.BlockSounds != null && collection.InteractiveSounds != null)
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
					}

					if (!string.IsNullOrWhiteSpace(soundCategory))
					{
						if (collection.BlockSounds != null && collection.InteractiveSounds != null)
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
					}

					if (collection.EntitySounds?.Defaults?.Events != null)
					{
						if (collection.EntitySounds.Defaults.Events.TryGetValue(soundEvent, out se))
						{
							if (!string.IsNullOrWhiteSpace(se.Sound))
							{
								sound = se.Sound;
							}
						}
					}
				}
			}

			if (string.IsNullOrWhiteSpace(sound))
			{
				Log.Debug(
					$"Failed to translate sound with id: {soundId} (Sound={sound}, SoundEvent={soundEvent}, blockid={blockId}, entityType={entityTypeId}, block={blockName}, soundCat={soundCategory})");

				return;
			}

			if (!AlexInstance.AudioEngine.PlaySound(
				    sound, new Microsoft.Xna.Framework.Vector3(position.X, position.Y, position.Z), pitch, volume,
				    isGlobal))
			{
				Log.Debug($"Could not play sound: {sound}");
			}
		}

		public void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
		{
			PlaySound(message.soundId, message.blockId, message.entityType, message.position, message.isGlobal);
			//UnhandledPackage(message);
		}

		public void HandleMcpePlaySound(McpePlaySound message)
		{
			var coords = message.coordinates;

			if (!AlexInstance.AudioEngine.PlaySound(
				    message.name, new Microsoft.Xna.Framework.Vector3(coords.X, coords.Y, coords.Z), message.pitch,
				    message.volume))
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

			switch ((TitleType)message.type)
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

					break;

				case TitleType.SubTitle:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.SetSubtitle(message.text);

					break;

				case TitleType.ActionBar:

					break;

				case TitleType.AnimationTimes:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.Show();

					break;
			}
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
			if (_playerListPlayers.TryGetValue(message.uuid, out var data))
			{
				data.Skin = message.skin;
			}

			if (Client.World.EntityManager.TryGet(message.uuid, out var entity) && entity is RemotePlayer player)
			{
				player.SetSkin(message.skin);
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

					scoreboard.AddObjective(
						new ScoreboardObjective(
							message.objectiveName, message.displayName, message.sortOrder, message.criteriaName));

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
						objective.AddOrUpdate(
							fakePlayer.Id.ToString(),
							new ScoreboardEntry(fakePlayer.Id.ToString(), fakePlayer.Score, fakePlayer.CustomName));
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
								ent.Id.ToString(), new ScoreboardEntry(ent.Id.ToString(), ent.Score, entity.NameTag));
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
				else { }
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
				BlockUpdatePriority priority = (BlockUpdatePriority)message.blockPriority;

				Client.World?.SetBlockState(
					new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z),
					converted, (int)message.dataLayerId, priority);
			}
			else
			{
				//Log.Warn($"Received unknown block runtime id.");
			}
		}

		public void HandleMcpeSetScoreboardIdentity(McpeSetScoreboardIdentity message)
		{
			UnhandledPackage(message);
		}

		public void HandleMcpeUpdateSoftEnum(McpeUpdateSoftEnum message)
		{
			//Client.CommandProvider.
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
			Log.Info($"Custom script event! Name={message.eventName} Data={message.eventData}");
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
				}

					return true;

				case McpeAnimateEntity emote:
				{
					HandleMcpeAnimateEntity(emote);
				}

					return true;

				case McpeUpdateGm packet:
				{
					HandleMcpeUpdatePlayerGameType(packet);
				}

					return true;
			}

			UnhandledPackage(message);

			return false;
		}

		public void HandleMcpeUpdatePlayerGameType(McpeUpdateGm packet)
		{
			if (packet.PlayerEntityUniqueId != Client.EntityId)
				return;

			Client.World.Player.UpdateGamemode((Interfaces.GameMode)packet.GameMode);
			//packet.
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

				Log.Warn($"Unhandled packet: {type.FullName} * {p.Value.count} (0x{p.Key:x2})");
			}
		}
	}
}