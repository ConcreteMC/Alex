using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Blocks.State;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Entities;
using Alex.GameStates;
using Alex.Graphics.Models.Entity;
using Alex.Items;
using Alex.ResourcePackLib.Json.Models.Entities;
using Alex.Utils;
using fNbt;
using Microsoft.Xna.Framework.Graphics;
using MiNET;
using MiNET.Client;
using MiNET.Net;
using MiNET.Utils;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using Inventory = Alex.Utils.Inventory;
using NibbleArray = MiNET.Utils.NibbleArray;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using UUID = Alex.API.Utils.UUID;

namespace Alex.Worlds.Bedrock
{
	public class BedrockClientPacketHandler : McpeClientMessageHandlerBase
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(BedrockClientPacketHandler));

		private BedrockClient BaseClient { get; }
		private Alex AlexInstance { get; }
        private CancellationToken CancellationToken { get; }
        private ChunkProcessor ChunkProcessor { get; }

        public BedrockClientPacketHandler(BedrockClient client, Alex alex, CancellationToken cancellationToken) :
	        base(client)
        {
	        BaseClient = client;
	        AlexInstance = alex;
	        CancellationToken = cancellationToken;

	        AnvilWorldProvider.LoadBlockConverter();

	        ChunkProcessor = new ChunkProcessor(4,
		        alex.Services.GetService<IOptionsProvider>().AlexOptions.MiscelaneousOptions.ServerSideLighting,
		        cancellationToken);
        }

        private void UnhandledPackage(Packet packet)
		{
			Log.Warn($"Unhandled bedrock packet: {packet.GetType().Name} (0x{packet.Id:X2})");
		}

		public override void HandleMcpePlayStatus(McpePlayStatus message)
		{
			Client.PlayerStatus = message.status;

			if (Client.PlayerStatus == 3)
			{
				Client.HasSpawned = true;

				Client.PlayerStatusChangedWaitHandle.Set();

				Client.SendMcpeMovePlayer();
				
				var packet = McpeSetLocalPlayerAsInitializedPacket.CreateObject();
				packet.runtimeEntityId = BaseClient.EntityId;
				BaseClient.SendPacket(packet);
			}
		}

        public override void HandleMcpeDisconnect(McpeDisconnect message)
        {
            Log.Info($"Received disconnect: {message.message}");
            BaseClient.ShowDisconnect(message.message, false);
            base.HandleMcpeDisconnect(message);
        }

        public override void HandleMcpeText(McpeText message)
		{
			BaseClient.WorldProvider?.GetChatReceiver?.Receive(new ChatObject(message.message));
		}

		public override void HandleMcpeSetTime(McpeSetTime message)
		{
			BaseClient.WorldReceiver?.SetTime(message.time);
			_changeDimensionResetEvent.Set();
		}

		private IReadOnlyDictionary<uint, Blockstate> _blockStateMap;
		public override void HandleMcpeStartGame(McpeStartGame message)
		{
			Client.EntityId = message.runtimeEntityId;
			Client.NetworkEntityId = message.entityIdSelf;
			Client.SpawnPoint = message.spawn;
			Client.CurrentLocation = new MiNET.Utils.PlayerLocation(Client.SpawnPoint, message.unknown1.X, message.unknown1.X, message.unknown1.Y);

			BaseClient.WorldReceiver?.UpdatePlayerPosition(new API.Utils.PlayerLocation(new Microsoft.Xna.Framework.Vector3(Client.SpawnPoint.X, Client.SpawnPoint.Y, Client.SpawnPoint.Z), message.unknown1.X, message.unknown1.X, message.unknown1.Y));

			Dictionary<uint, Blockstate> ourStates = new Dictionary<uint, Blockstate>();
			foreach (var blockstate in message.blockstates)
			{
				var name = blockstate.Value.Name;

				if (name.Equals("minecraft:grass", StringComparison.InvariantCultureIgnoreCase))
					name = "minecraft:grass_block";

				blockstate.Value.Name = name;
				
				//var state = BlockFactory.GetBlockState(name);
				ourStates.TryAdd((uint)blockstate.Key, blockstate.Value);
			}
			
			_blockStateMap = ourStates;
			ChunkProcessor._blockStateMap = ourStates;
			
			//File.WriteAllText("blockies.json", JsonConvert.SerializeObject(message.blockstates, Formatting.Indented));
			{
				BaseClient.RequestChunkRadius(Client.ChunkRadius);
			}

            if (BaseClient.WorldReceiver?.GetPlayerEntity() is Player player)
            {
                player.Inventory.IsPeInventory = true;
            }
        }

		public override void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				BaseClient.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, 
					new PlayerLocation(message.x, message.y - Player.EyeLevel, message.z, message.headYaw, message.yaw, message.pitch));
				return;
			}
			
			BaseClient.WorldReceiver.UpdatePlayerPosition(new 
				PlayerLocation(message.x, message.y, message.z));

			//BaseClient.SendMcpeMovePlayer();
			Client.CurrentLocation = new MiNET.Utils.PlayerLocation(message.x, message.y, message.z);
			BaseClient.SendMcpeMovePlayer();
		}


		public override void HandleMcpeAdventureSettings(McpeAdventureSettings message)
		{
			base.HandleMcpeAdventureSettings(message);
			if (BaseClient.WorldReceiver.GetPlayerEntity() is Player player)
			{
				player.CanFly = ((message.flags & 0x40) == 0x40);
				player.IsFlying = ((message.flags & 0x200) == 0x200);
			}
		}

		private ConcurrentDictionary<UUID, PlayerMob> _players = new ConcurrentDictionary<UUID, PlayerMob>();
        public override void HandleMcpeAddPlayer(McpeAddPlayer message)
		{
			UUID u = new UUID(message.uuid.GetBytes());
			if (_players.TryGetValue(u, out PlayerMob mob))
			{
				mob.EntityId = message.runtimeEntityId;
				mob.KnownPosition = new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch);

				if (BaseClient.WorldReceiver is World w)
				{
					mob.IsSpawned = true;
					w.SpawnEntity(mob.EntityId, mob);
				}
				else
				{
					mob.IsSpawned = false;
				}
			}
		}

		public override void HandleMcpePlayerList(McpePlayerList message)
		{
			if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					var u = new API.Utils.UUID(r.ClientUuid.GetBytes());
					if (_players.ContainsKey(u)) continue;

					Texture2D skinTexture;
					if (r.Skin.TryGetBitmap(out Bitmap skinBitmap))
					{
						skinTexture =
							TextureUtils.BitmapToTexture2D(BaseClient.WorldProvider.Alex.GraphicsDevice, skinBitmap);
					}
					else
					{
						BaseClient.WorldProvider.Alex.Resources.ResourcePack.TryGetBitmap("entity/alex", out Bitmap rawTexture);
						skinTexture = TextureUtils.BitmapToTexture2D(BaseClient.WorldProvider.Alex.GraphicsDevice, rawTexture);
					}
					
                    BaseClient.WorldReceiver?.AddPlayerListItem(new PlayerListItem(u, r.DisplayName, Gamemode.Survival, 0));
					PlayerMob m = new PlayerMob(r.DisplayName, BaseClient.WorldReceiver as World, BaseClient, skinTexture, true);

					if (!_players.TryAdd(u, m))
					{
						Log.Warn($"Duplicate player record! {r.ClientUuid}");
					}
				}
			}
            else if (message.records is PlayerRemoveRecords removeRecords)
            {
	            foreach (var r in removeRecords)
	            {
		            var u = new UUID(r.ClientUuid.GetBytes());
		            if (_players.TryRemove(u, out var player))
		            {
			            BaseClient.WorldReceiver?.RemovePlayerListItem(u);
			            if (BaseClient.WorldReceiver is World w)
			            {
				            w.DespawnEntity(player.EntityId);
			            }
		            }
	            }
            }
		}

		public void SpawnMob(long entityId, Guid uuid, EntityType type, PlayerLocation position, Microsoft.Xna.Framework.Vector3 velocity)
		{
			Entity entity = null;
			if (EntityFactory.ModelByType(type, out var renderer, out EntityData knownData))
			{
				//if (Enum.TryParse(knownData.Name, out type))
				//{
				//	entity = type.Create(null);
				//}
                entity = type.Create(null);

				if (entity == null)
				{
					entity = new Entity((int) type, null, BaseClient);
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
				var def = AlexInstance.Resources.BedrockResourcePack.EntityDefinitions.FirstOrDefault(x =>
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

						if (AlexInstance.Resources.BedrockResourcePack.Textures.TryGetValue(texture,
							out Bitmap bmp))
						{
							Texture2D t = TextureUtils.BitmapToTexture2D(AlexInstance.GraphicsDevice, bmp);

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


			BaseClient.WorldProvider.SpawnEntity(entityId, entity);
		}


		public override void HandleMcpeAddEntity(McpeAddEntity message)
		{
			var type = message.entityType.Replace("minecraft:", "");
            if (Enum.TryParse(typeof(EntityType), type, true, out object res))
            {
                SpawnMob(message.runtimeEntityId, Guid.NewGuid(), (EntityType)res, new PlayerLocation(message.x, message.y, message.z, message.headYaw, message.yaw, message.pitch), new Microsoft.Xna.Framework.Vector3(message.speedX, message.speedY, message.speedZ));
            }
            else
            {
                Log.Warn($"Unknown mob: {type}");
            }
        }

		public override void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			BaseClient.WorldReceiver?.DespawnEntity(message.entityIdSelf);
		}

		public override void HandleMcpeAddItemEntity(McpeAddItemEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeTakeItemEntity(McpeTakeItemEntity message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMoveEntity(McpeMoveEntity message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
                BaseClient.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId,
					new PlayerLocation(message.position.X, message.position.Y - Player.EyeLevel, message.position.Z, 
						message.position.HeadYaw, message.position.Yaw, message.position.Pitch));
				return;
			}

           // BaseClient.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, new PlayerLocation(message.position), true, true, true);
			UnhandledPackage(message);
		}

        public override void HandleMcpeMoveEntityDelta(McpeMoveEntityDelta message)
        {
            return;
          /*  if (message.runtimeEntityId != Client.EntityId)
            {
                //TODO: Fix delta reading on packets.
                BaseClient.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId,
                    new PlayerLocation(message.Delta), true, true, true);
                return;
            }*/
        }

        public override void HandleMcpeRiderJump(McpeRiderJump message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateBlock(McpeUpdateBlock message)
		{
			if (message.storage != 0)
			{
				Log.Warn($"UPDATEBLOCK: Unsupported block storage! {message.storage}");
				return;
			}
			
			if (_blockStateMap.TryGetValue(message.blockRuntimeId, out var bs))
			{
				IBlockState state = null;
				
				var result =
					BlockFactory.RuntimeIdTable.FirstOrDefault(xx =>
						xx.Name == bs.Name);

				if (result != null && result.Id >= 0)
				{
					var reverseMap = MiNET.Worlds.AnvilWorldProvider.Convert.FirstOrDefault(map =>
						map.Value.Item1 == result.Id);

					var id = result.Id;
					if (reverseMap.Value != null)
					{
						id = reverseMap.Key;
					}
													        
					var res = BlockFactory.GetBlockStateID(
						(int) id,
						(byte) bs.Data);

					if (AnvilWorldProvider.BlockStateMapper.TryGetValue(
						res,
						out var res2))
					{
														        
						var t = BlockFactory.GetBlockState(res2);
						t = ChunkProcessor.TranslateBlockState(t, id,
							bs.Data);

						state = t;
					}
					else
					{
						Log.Info(
							$"Did not find anvil statemap: {result.Name}");
						state = ChunkProcessor.TranslateBlockState(
							BlockFactory.GetBlockState(result.Name),
							id, bs.Data);
					}
				}

				if (state == null)
				{
					state = ChunkProcessor.TranslateBlockState(
						BlockFactory.GetBlockState(bs.Name),
						-1, bs.Data);
				}

				if (state != null)
					BaseClient.WorldReceiver?.SetBlockState(
						new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z), 
						state);
				
				/*
				var result =
					BlockFactory.RuntimeIdTable.FirstOrDefault(xx => xx.Name == bs.Name);
				
				uint res = 0;
				bool ss = false;
				if (result != null && result.Id >= 0)
				{
					res = BlockFactory.GetBlockStateID((int) result.Id, (byte) bs.Data);
					ss = true;
				}

				if (ss && AnvilWorldProvider.BlockStateMapper.TryGetValue(res, out res))
				{
					var a = BlockFactory.GetBlockState(res);
					BaseClient.WorldReceiver?.SetBlockState(
						new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z), 
						a);
				}
				else
				{

					BaseClient.WorldReceiver?.SetBlockState(
						new BlockCoordinates(message.coordinates.X, message.coordinates.Y, message.coordinates.Z),
						BlockFactory.GetBlockState(bs.Name));
				}*/
			}
			else
			{
				Log.Warn($"Received unknown block runtime id.");
			}
		}

		public override void HandleMcpeAddPainting(McpeAddPainting message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeExplode(McpeExplode message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeLevelSoundEventOld(McpeLevelSoundEventOld message)
		{
			UnhandledPackage(message);
        }

		public override void HandleMcpeSpawnParticleEffect(McpeSpawnParticleEffect message)
		{
			UnhandledPackage(message);
        }

		public override void HandleMcpeAvailableEntityIdentifiers(McpeAvailableEntityIdentifiers message)
		{
			UnhandledPackage(message);
        }

		public override void HandleMcpeLevelSoundEventV2(McpeLevelSoundEventV2 message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeNetworkChunkPublisherUpdate(McpeNetworkChunkPublisherUpdate message)
		{
			UnhandledPackage(message);
        }

		public override void HandleMcpeBiomeDefinitionList(McpeBiomeDefinitionList message)
		{
			UnhandledPackage(message);
        }

		public override void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeLevelEvent(McpeLevelEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBlockEvent(McpeBlockEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMobEffect(McpeMobEffect message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateAttributes(McpeUpdateAttributes message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInventoryTransaction(McpeInventoryTransaction message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInteract(McpeInteract message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeHurtArmor(McpeHurtArmor message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetEntityData(McpeSetEntityData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetEntityMotion(McpeSetEntityMotion message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetEntityLink(McpeSetEntityLink message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetHealth(McpeSetHealth message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetSpawnPosition(McpeSetSpawnPosition message)
		{
			Client.SpawnPoint = new Vector3(message.coordinates.X, message.coordinates.Y, message.coordinates.Z);
			Client.LevelInfo.SpawnX = (int)Client.SpawnPoint.X;
			Client.LevelInfo.SpawnY = (int)Client.SpawnPoint.Y;
			Client.LevelInfo.SpawnZ = (int)Client.SpawnPoint.Z;

			
			//		Client.SpawnPoint = new Vector3(message.);
		}

		public override void HandleMcpeAnimate(McpeAnimate message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeContainerOpen(McpeContainerOpen message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeContainerClose(McpeContainerClose message)
		{
			UnhandledPackage(message);
		}

        public override void HandleMcpeInventoryContent(McpeInventoryContent message)
		{
			Inventory inventory = null;
			if (message.inventoryId == 0x00)
			{
				if (BaseClient.WorldReceiver?.GetPlayerEntity() is Player player)
				{
					inventory = player.Inventory;
				}
			}

			if (inventory == null) return;

			for (var index = 0; index < message.input.Count; index++)
			{
				var slot = message.input[index];
				
				var usedIndex = index;

                if (ItemFactory.TryGetItem(slot.Id, slot.Metadata, out Item item))
				{
                    item.Count = slot.Count;
                    inventory[usedIndex] = item;
				}
                else
                {
                    Log.Warn($"Failed to set slot: {index} Id: {slot.Id}:{slot.Metadata}");
                }
            }
		}

		public override void HandleMcpeInventorySlot(McpeInventorySlot message)
		{
			Inventory inventory = null;
			if (message.inventoryId == 0x00)
			{
				if (BaseClient.WorldReceiver?.GetPlayerEntity() is Player player)
				{
					inventory = player.Inventory;
                    if (!inventory.IsPeInventory)
                        player.Inventory.IsPeInventory = true;
                }
			}

			if (inventory == null) return;
			
			var index = (int)message.slot;

            if (ItemFactory.TryGetItem(message.item.Id, message.item.Metadata, out Item item))
            {
                item.Count = message.item.Count;
				inventory[index] = item;
              //  Log.Info($"Set inventory slot: {message.slot} Id: {message.item.Id}:{message.item.Metadata} x {message.item.Count} Name: {item.DisplayName} IsPeInv: {inventory.IsPeInventory}");
            }
            else
            {
                Log.Warn($"Failed to set slot: {message.slot} Id: {message.item.Id}:{message.item.Metadata}");
            }
		}

        public override void HandleMcpePlayerHotbar(McpePlayerHotbar message)
        {
            UnhandledPackage(message);
        }

        public override void HandleMcpeContainerSetData(McpeContainerSetData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCraftingData(McpeCraftingData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCraftingEvent(McpeCraftingEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeGuiDataPickItem(McpeGuiDataPickItem message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBlockEntityData(McpeBlockEntityData message)
		{
			UnhandledPackage(message);
		}

		private AutoResetEvent _changeDimensionResetEvent = new AutoResetEvent(false);
		public override void HandleMcpeChangeDimension(McpeChangeDimension message)
		{
			base.HandleMcpeChangeDimension(message);
			if (BaseClient.WorldProvider is BedrockWorldProvider provider)
			{
				LoadingWorldState loadingWorldState = new LoadingWorldState();
				AlexInstance.GameStateManager.SetActiveState(loadingWorldState, true);
				loadingWorldState.UpdateProgress(LoadingState.LoadingChunks, 0);
				
				
				foreach (var loadedChunk in provider.LoadedChunks)
				{
					provider.UnloadChunk(loadedChunk);
				}
				
				ThreadPool.QueueUserWorkItem(async o =>
				{
					double radiusSquared = Math.Pow(Client.ChunkRadius, 2);
					var target = radiusSquared * 3;

					int percentage = 0;
					bool ready = false;
					
					do
					{
						percentage = (int) (provider.LoadedChunks.Count() / target) * 100;
						
						loadingWorldState.UpdateProgress(LoadingState.LoadingChunks,
							percentage);

						if (!ready)
						{
							if (_changeDimensionResetEvent.WaitOne(0))
								ready = true;
						}
						else
						{
							await Task.Delay(50);
						}
						
					} while (!ready || percentage < 99);
					
					AlexInstance.GameStateManager.Back();
				});
			}
		}
		
		public override void HandleMcpeFullChunkData(McpeFullChunkData msg)
		{
			var chunkData = msg.chunkData;
			var cx = msg.chunkX;
			var cz = msg.chunkZ;
			msg.PutPool();
			
			if (chunkData[0] < 1) //Nothing to read.
				return;
			
			ChunkProcessor.HandleChunkData(chunkData, cx, cz, BaseClient.ChunkReceived);
		}

		public override void HandleMcpeSetCommandsEnabled(McpeSetCommandsEnabled message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetDifficulty(McpeSetDifficulty message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetPlayerGameType(McpeSetPlayerGameType message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSimpleEvent(McpeSimpleEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeTelemetryEvent(McpeTelemetryEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSpawnExperienceOrb(McpeSpawnExperienceOrb message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeClientboundMapItemData(McpeClientboundMapItemData message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
		{
			BaseClient.RequestChunkRadius(Client.ChunkRadius);
		}

		public override void HandleMcpeChunkRadiusUpdate(McpeChunkRadiusUpdate message)
		{
			Client.ChunkRadius = message.chunkRadius;
			//UnhandledPackage(message);
		}

		public override void HandleMcpeItemFrameDropItem(McpeItemFrameDropItem message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeGameRulesChanged(McpeGameRulesChanged message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeCamera(McpeCamera message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBossEvent(McpeBossEvent message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeShowCredits(McpeShowCredits message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeAvailableCommands(McpeAvailableCommands message)
		{
			BaseClient.LoadCommands(message.CommandSet);
			//UnhandledPackage(message);
		}

		public override void HandleMcpeCommandOutput(McpeCommandOutput message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateTrade(McpeUpdateTrade message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateEquipment(McpeUpdateEquipment message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeTransfer(McpeTransfer message)
		{
			BaseClient.SendDisconnectionNotification();
			BaseClient.StopClient();
			
			IPHostEntry hostEntry = Dns.GetHostEntry(message.serverAddress);

			if (hostEntry.AddressList.Length > 0)
			{
				var ip = hostEntry.AddressList[0];
				AlexInstance.ConnectToServer(new IPEndPoint(ip, message.port), BaseClient.PlayerProfile, true);
			}
		}

		public override void HandleMcpePlaySound(McpePlaySound message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeStopSound(McpeStopSound message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetTitle(McpeSetTitle message)
		{
			var titleComponent = BaseClient.WorldProvider?.TitleComponent;
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
					titleComponent.SetTitle(new ChatObject(message.text));
					titleComponent.Show();
					break;
				case TitleType.SubTitle:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					titleComponent.SetSubtitle(new ChatObject(message.text));
					titleComponent.Show();
					break;
				case TitleType.ActionBar:
					
					break;
				case TitleType.AnimationTimes:
					titleComponent.SetTimes(message.fadeInTime, message.stayTime, message.fadeOutTime);
					break;
			}
	
			//UnhandledPackage(message);
		}

		public override void HandleMcpeAddBehaviorTree(McpeAddBehaviorTree message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeStructureBlockUpdate(McpeStructureBlockUpdate message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeShowStoreOffer(McpeShowStoreOffer message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpePlayerSkin(McpePlayerSkin message)
		{
			//TODO: Load skin
			UnhandledPackage(message);
		}

		public override void HandleMcpeSubClientLogin(McpeSubClientLogin message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeInitiateWebSocketConnection(McpeInitiateWebSocketConnection message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetLastHurtBy(McpeSetLastHurtBy message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeBookEdit(McpeBookEdit message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeNpcRequest(McpeNpcRequest message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeModalFormRequest(McpeModalFormRequest message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeServerSettingsResponse(McpeServerSettingsResponse message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeShowProfile(McpeShowProfile message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetDefaultGameType(McpeSetDefaultGameType message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeRemoveObjective(McpeRemoveObjective message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetDisplayObjective(McpeSetDisplayObjective message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeSetScore(McpeSetScore message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeLabTable(McpeLabTable message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateBlockSynced(McpeUpdateBlockSynced message)
		{
			UnhandledPackage(message);
		}

        public override void HandleMcpeSetScoreboardIdentityPacket(McpeSetScoreboardIdentityPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeUpdateSoftEnumPacket(McpeUpdateSoftEnumPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeNetworkStackLatencyPacket(McpeNetworkStackLatencyPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleMcpeScriptCustomEventPacket(McpeScriptCustomEventPacket message)
		{
			UnhandledPackage(message);
		}

		public override void HandleFtlCreatePlayer(FtlCreatePlayer message)
		{
			UnhandledPackage(message);
		}
	}
}
