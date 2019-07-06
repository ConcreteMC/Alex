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
		public BedrockClientPacketHandler(BedrockClient client, Alex alex) : base(client)
		{
			BaseClient = client;
			AlexInstance = alex;
			
			AnvilWorldProvider.LoadBlockConverter();
        }

		private void UnhandledPackage(Packet packet)
		{
			Log.Warn($"Unhandled bedrock packet: {packet.GetType().Name} (0x{packet.Id:X2})");
		}

		public override void HandleMcpePlayStatus(McpePlayStatus message)
		{
			base.HandleMcpePlayStatus(message);

            if (message.status == 3)
            {
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
					
                    BaseClient.WorldReceiver?.AddPlayerListItem(new PlayerListItem(u, r.Username, Gamemode.Survival, 0));
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
				}
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
		
		enum PalleteType : byte
		{
			Paletted1 = 1,   // 32 blocks per word, max 2 unique blockstates
			Paletted2 = 2,   // 16 blocks per word, max 4 unique blockstates
			Paletted3 = 3,   // 10 blocks and 2 bits of padding per word, max 8 unique blockstates
			Paletted4 = 4,   // 8 blocks per word, max 16 unique blockstates
			Paletted5 = 5,   // 6 blocks and 2 bits of padding per word, max 32 unique blockstates
			Paletted6 = 6,   // 5 blocks and 2 bits of padding per word, max 64 unique blockstates
			Paletted8  = 8,  // 4 blocks per word, max 256 unique blockstates
			Paletted16 = 16, // 2 blocks per word, max 65536 unique blockstates
		}

		public uint SwapBytes(uint x)
		{
			// swap adjacent 16-bit blocks
			x = (x >> 16) | (x << 16);
			// swap adjacent 8-bit blocks
			return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
		}
		
		private static ConcurrentDictionary<uint, IBlockState> _convertedStates = new ConcurrentDictionary<uint, IBlockState>();
		public override void HandleMcpeFullChunkData(McpeFullChunkData message)
		{
			var chunkData = message.chunkData;
			var cx = message.chunkX;
			var cz = message.chunkZ;
			message.PutPool();
			
			ThreadPool.QueueUserWorkItem(o =>
			{
				try
				{
					using (MemoryStream stream = new MemoryStream(chunkData))
					{
						NbtBinaryReader defStream = new NbtBinaryReader(stream, true);

						int count = defStream.ReadByte();
						if (count < 1)
						{
							Log.Warn("Nothing to read");
							return;
						}

						ChunkColumn chunkColumn = new ChunkColumn();
						chunkColumn.IsDirty = true;
						chunkColumn.X = cx;
						chunkColumn.Z = cz;

						for (int s = 0; s < count; s++)
						{
							var section = chunkColumn.Sections[s] as ChunkSection;
							if (section == null) section = new ChunkSection(s, true);

							int version = defStream.ReadByte();

							if (version == 1 || version == 8)
							{
								int storageSize = defStream.ReadByte();

								for (int storage = 0; storage < storageSize; storage++)
								{
									int paletteAndFlag = defStream.ReadByte();
									bool isRuntime = (paletteAndFlag & 1) != 0;
									int bitsPerBlock = paletteAndFlag >> 1;
									int blocksPerWord = (int) Math.Floor(32f / bitsPerBlock);
									int wordCount = (int) Math.Ceiling(4096.0f / blocksPerWord);

									uint[] words = new uint[wordCount];
									for (int w = 0; w < wordCount; w++)
									{
										int word = defStream.ReadInt32();
										words[w] = SwapBytes((uint)word);
									}

									uint[] pallete = new uint[0];

									if (isRuntime)
									{
										int palleteSize = VarInt.ReadSInt32(stream);
										pallete = new uint[palleteSize];
										//var copy = new uint[pallete.Length];
										for (int pi = 0; pi < pallete.Length; pi++)
										{
											var ui = (uint) VarInt.ReadSInt32(stream);
											pallete[pi] = ui;
											//copy[(pallete.Length - pi) - 1] = ui;
										}

										if (palleteSize == 0)
										{
											Log.Warn($"Pallete size is 0");
											continue;
										}
										//Array.Copy(copy, 0, pallete, 0, pallete.Length );
									}

									//if (storage > 0) continue;
									//long afterPaletteIndex = defStream.BaseStream.Position;

									int position = 0;
									//for (int w = words.Length - 1; w > 0; w--)
									for(int w = 0; w < wordCount; w++)
									{
										 uint word = words[w];
										for (int block = 0; block < blocksPerWord; block++)
										{
											if (position >= 4096) break; // padding bytes

											uint state =(uint)( (word >> ((position % blocksPerWord) * bitsPerBlock)) &
											            ((1 << bitsPerBlock) - 1));
											int x = (position >> 8) & 0xF;
											int y = position & 0xF; 
											int z = (position >> 4) & 0xF;

											if (storage == 0)
											{
												if (state >= pallete.Length)
												{
													continue;
												}

												IBlockState translated = _convertedStates.GetOrAdd(pallete[state], u =>
												{
													if (_blockStateMap.TryGetValue(pallete[state], out var bs))
													{

														var result =
															BlockFactory.RuntimeIdTable.FirstOrDefault(xx =>
																xx.Name == bs.Name);

														if (result != null && result.Id >= 0)
														{
															var res = BlockFactory.GetBlockStateID((int) result.Id,
																(byte) bs.Data);

															if (AnvilWorldProvider.BlockStateMapper.TryGetValue(res,
																out var res2))
															{
																var t = BlockFactory.GetBlockState(res2);
																t = TranslateBlockState(t, result.Id,
																	bs.Data);

																return t;
															}
															else
															{
																Log.Info($"Did not find anvil statemap: {result.Name}");
																return TranslateBlockState(BlockFactory.GetBlockState(result.Name),
																	result.Id, bs.Data);
															}
														}

														return TranslateBlockState(BlockFactory.GetBlockState(bs.Name),
															-1, bs.Data);
													}

													return null;
												});

												if (translated != null)
												{
													section.Set(x, y, z, translated);
												}
											}
											else
											{
												//TODO.
											}

											position++;
										}
										if (position >= 4096) break; 
									}
								}
							}
							else
							{
								#region OldFormat 

								byte[] blockIds = new byte[4096];
								defStream.Read(blockIds, 0, blockIds.Length);

								NibbleArray data = new NibbleArray(4096);
								defStream.Read(data.Data, 0, data.Data.Length);

								for (int x = 0; x < 16; x++)
								{
									for (int z = 0; z < 16; z++)
									{
										for (int y = 0; y < 16; y++)
										{
											int idx = (x << 8) + (z << 4) + y;
											var id = blockIds[idx];
											var meta = data[idx];

											IBlockState result = null;

											if (id > 0 && result == null)
											{
												var res = BlockFactory.GetBlockStateID(id, meta);

												if (AnvilWorldProvider.BlockStateMapper.TryGetValue(res,
													out var res2))
												{
													var t = BlockFactory.GetBlockState(res2);
													t = TranslateBlockState(t, id,
														meta);

													result = t;
												}
												else
												{
													Log.Info($"Did not find anvil statemap: {result.Name}");
													result = TranslateBlockState(BlockFactory.GetBlockState(res),
														id, meta);
												}
											}

											if (result == null)
											{
												var results = BlockFactory.RuntimeIdTable.Where(xx =>
													xx.Id == id && xx.Data == meta).ToArray();

												if (results.Length > 0)
												{
													result = TranslateBlockState(
														BlockFactory.GetBlockState((uint) results[0].RuntimeId), id, meta);
												}
											}

											if (result != null)
											{
												section.Set(x, y, z, result);
											}
										}
									}
								}

								#endregion
							}

							if (AlexInstance.GameSettings.UseAlexChunks)
							{
								//  Log.Info($"Alex chunk!");

								Utils.NibbleArray skyLight = new Utils.NibbleArray(4096);
								var rawSky = new Utils.NibbleArray(4096);
								defStream.Read(rawSky.Data, 0, rawSky.Data.Length);

								Utils.NibbleArray blockLight = new Utils.NibbleArray(4096);
								var rawBlock = new Utils.NibbleArray(4096);
								defStream.Read(rawBlock.Data, 0, rawBlock.Data.Length);

								for (int x = 0; x < 16; x++)
								for (int y = 0; y < 16; y++)
								for (int z = 0; z < 16; z++)
								{
									var peIndex = (x * 256) + (z * 16) + y;
									var sky = rawSky[peIndex];
									var block = rawBlock[peIndex];

									var idx = y << 8 | z << 4 | x;
									skyLight[idx] = sky;
									blockLight[idx] = block;
								}

								section.BlockLight = blockLight;
								section.SkyLight = skyLight;
							}

							section.RemoveInvalidBlocks();
							section.IsDirty = true;
							//section.ScheduledUpdates = new bool[16 * 16 * 16];
							//section.ScheduledSkylightUpdates = new bool[16 * 16 * 16];
							//Make sure the section is saved.
							chunkColumn.Sections[s] = section;
						}

						//if (stream.Position >= stream.Length - 1) continue;


						byte[] ba = new byte[512];
						if (defStream.Read(ba, 0, 256 * 2) != 256 * 2) Log.Error($"Out of data height");

						Buffer.BlockCopy(ba, 0, chunkColumn.Height, 0, 512);
						//Log.Debug($"Heights:\n{Package.HexDump(ba)}");

						//if (stream.Position >= stream.Length - 1) continue;

						int[] biomeIds = new int[256];
						for (int i = 0; i < biomeIds.Length; i++)
						{
							biomeIds[i] = defStream.ReadByte();
						}

						chunkColumn.BiomeId = biomeIds;
						//if (defStream.Read(chunkColumn.BiomeId, 0, 256) != 256) Log.Error($"Out of data biomeId");
						//Log.Debug($"biomeId:\n{Package.HexDump(chunk.biomeId)}");

						if (stream.Position >= stream.Length - 1)
						{
							BaseClient.ChunkReceived(chunkColumn);
							return;
						}

						int borderBlock = VarInt.ReadSInt32(stream);
						if (borderBlock > 0)
						{
							byte[] buf = new byte[borderBlock];
							int len = defStream.Read(buf, 0, borderBlock);
							Log.Warn($"??? Got borderblock {borderBlock}. Read {len} bytes");
							Log.Debug($"{Packet.HexDump(buf)}");
							for (int i = 0; i < borderBlock; i++)
							{
								int x = (buf[i] & 0xf0) >> 4;
								int z = buf[i] & 0x0f;
								Log.Debug($"Border block: x={x}, z={z}");
							}
						}


						if (stream.Position < stream.Length - 1)
						{
							//Log.Debug($"Got NBT data\n{Package.HexDump(defStream.ReadBytes((int) (stream.Length - stream.Position)))}");

							while (stream.Position < stream.Length)
							{
								NbtFile file = new NbtFile()
								{
									BigEndian = false,
									UseVarInt = true
								};

								file.LoadFromStream(stream, NbtCompression.None);

								Log.Debug($"Blockentity: {file.RootTag}");
							}
						}

						if (stream.Position < stream.Length - 1)
						{
							Log.Warn(
								$"Still have data to read\n{Packet.HexDump(defStream.ReadBytes((int) (stream.Length - stream.Position)))}");
						}

						//Done processing this chunk, send to world
						BaseClient.ChunkReceived(chunkColumn);
					}

				}
				catch (Exception ex)
				{
					Log.Error($"Exception in chunk loading: {ex.ToString()}");
				}
			});
		}
		
		const string facing = "facing";
		private IBlockState FixFacing(IBlockState state, int meta)
		{
			switch (meta)
			{
				case 4:
				case 0:
					state = state.WithProperty(facing, "east");
					break;
				case 5:
				case 1:
					state = state.WithProperty(facing, "west");
					break;
				case 6:
				case 2:
					state = state.WithProperty(facing, "south");
					break;
				case 7:
				case 3:
					state = state.WithProperty(facing, "north");
					break;
			}

			return state;
		}
		
		private static string[] _slabs = new string[]
		{
			"minecraft:stone_slab",
			"minecraft:smooth_stone_slab",
			"minecraft:stone_brick_slab",
			"minecraft:sandstone_slab",
			"minecraft:acacia_slab",
			"minecraft:birch_slab",
			"minecraft:dark_oak_slab",
			"minecraft:jungle_slab",
			"minecraft:oak_slab",
			"minecraft:spruce_slab",
			"minecraft:purpur_slab",
			"minecraft:quartz_slab",
			"minecraft:red_sandstone_slab",
			"minecraft:brick_slab",
			"minecraft:cobblestone_slab",
			"minecraft:nether_brick_slab",
			"minecraft:petrified_oak_slab",
			"minecraft:prismarine_slab",
			"minecraft:prismarine_brick_slab",
			"minecraft:dark_prismarine_slab",
			"minecraft:polished_granite_slab",
			"minecraft:smooth_red_sandstone_slab",
			"minecraft:mossy_stone_brick_slab",
			"minecraft:polished_diorite_slab",
			"minecraft:mossy_cobblestone_slab",
			"minecraft:end_stone_brick_slab",
			"minecraft:smooth_sandstone_slab",
			"minecraft:smooth_quartz_slab",
			"minecraft:granite_slab",
			"minecraft:andesite_slab",
			"minecraft:red_nether_brick_slab",
			"minecraft:polished_andesite_slab",
			"minecraft:diorite_slab",
			"minecraft:cut_sandstone_slab",
			"minecraft:cut_red_sandstone_slab"
		};
		
		private IBlockState TranslateBlockState(IBlockState state, long bid, int meta)
		{
			//var dict = state.ToDictionary();

			if (bid >= 8 && bid <= 11) //water or lava
			{
				state = state.WithProperty("level", meta.ToString());
			}
			else if (bid == 44 || bid == 182 || bid == 126 /*|| _slabs.Any(x => x.Equals(state.Name, StringComparison.InvariantCultureIgnoreCase))*/) //Slabs
			{
				var isUpper = (meta & 0x08) == 0x08;
				state = state.WithProperty("type", isUpper ? "top" : "bottom", true);
				
			} 
			else if (bid == 77 || bid == 143) //Buttons
			{
				switch (meta)
				{
					case 0:
					case 4:
						state = state.WithProperty(facing, "west");
						break;
					case 1:
					case 5:
						state = state.WithProperty(facing, "east");
						break;
					case 6:
					case 2:
						state = state.WithProperty(facing, "north");
						break;
					case 7:
					case 3:
						state = state.WithProperty(facing, "south");
						break;
				}
			}  
			//Stairs
			else if (bid == 163 || bid == 135 || bid == 108 || bid == 164 || bid == 136 || bid == 114 ||
			         bid == 53 ||
			         bid == 203 || bid == 156 || bid == 180 || bid == 128 || bid == 134 || bid == 109 || bid == 67)
			{
				//state = FixFacing(state, meta);
				
				state = ((BlockState)state).WithPropertyNoResolve("half", meta > 3 ? "top" : "bottom");
				
				switch (meta)
				{
					case 4:
					case 0:
						state = state.WithProperty(facing, "east", false, "waterlogged", "shape", "half");
						break;
					case 5:
					case 1:
						state = state.WithProperty(facing, "west", false, "waterlogged", "shape", "half");
						break;
					case 6:
					case 2:
						state = state.WithProperty(facing, "south", false, "waterlogged", "shape", "half");
						break;
					case 7:
					case 3:
						state = state.WithProperty(facing, "north", false, "waterlogged", "shape", "half");
						break;
				}
			}
			else if (bid == 96 || bid == 167 || state.Name.Contains("trapdoor")) //Trapdoors
			{
				state = FixFacing(state, meta);
				state = state.WithProperty("open", meta > 3 ? "true" : "false");
			}

			return state;
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
			UnhandledPackage(message);
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
