using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Alex.API.Blocks.State;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Blocks.Storage.Pallete;
using Alex.Entities;
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
using MiNET.Worlds;
using Newtonsoft.Json;
using NLog;
using BlockCoordinates = Alex.API.Utils.BlockCoordinates;
using Inventory = Alex.Utils.Inventory;
using NibbleArray = MiNET.Utils.NibbleArray;
using Player = Alex.Entities.Player;
using PlayerLocation = Alex.API.Utils.PlayerLocation;
using Skin = MiNET.Utils.Skins.Skin;
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
		}

		public override void HandleMcpeText(McpeText message)
		{
			BaseClient.WorldProvider?.GetChatReceiver?.Receive(new ChatObject(message.message));
		}

		public override void HandleMcpeSetTime(McpeSetTime message)
		{
			BaseClient.WorldReceiver?.SetTime(message.time);
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
			BaseClient.WorldProvider.Alex.Resources.ResourcePack.TryGetBitmap("entity/alex", out Bitmap rawTexture);
			var t = TextureUtils.BitmapToTexture2D(BaseClient.WorldProvider.Alex.GraphicsDevice, rawTexture);

            if (message.records is PlayerAddRecords addRecords)
			{
				foreach (var r in addRecords)
				{
					var u = new API.Utils.UUID(r.ClientUuid.GetBytes());
					if (_players.ContainsKey(u)) continue;
					
                    BaseClient.WorldReceiver?.AddPlayerListItem(new PlayerListItem(u, r.Username, Gamemode.Survival, 0));
					PlayerMob m = new PlayerMob(r.DisplayName, BaseClient.WorldReceiver as World, BaseClient, t, true);
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

		/*public void SpawnMob(int entityId, Guid uuid, EntityType type, PlayerLocation position, Microsoft.Xna.Framework.Vector3 velocity)
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
		}*/


		public override void HandleMcpeAddEntity(McpeAddEntity message)
		{
			//TODO: Spawn entitys
			UnhandledPackage(message);
		}

		public override void HandleMcpeRemoveEntity(McpeRemoveEntity message)
		{
			UnhandledPackage(message);
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
			UnhandledPackage(message);
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

		public override void HandleMcpePlayerHotbar(McpePlayerHotbar message)
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
				if (usedIndex < 9)
				{
					usedIndex += 36;
				}
				
				if (ItemFactory.TryGetItem(slot.Id, slot.Metadata, out Item item))
				{
					inventory[usedIndex] = item;
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
				}
			}

			if (inventory == null) return;
			
			var index = (int)message.slot;
			if (index <= 8)
			{
				index += 36;
			}
			else if (index >= 9 && index <= 35)
			{
				
			}
			
			if (ItemFactory.TryGetItem(message.item.Id, message.item.Metadata, out Item item))
			{
				inventory[index] = item;
			}
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

		public override void HandleMcpeFullChunkData(McpeFullChunkData message)
		{
			using (MemoryStream stream = new MemoryStream(message.chunkData))
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
				chunkColumn.X = message.chunkX;
				chunkColumn.Z = message.chunkZ;

				for (int s = 0; s < count; s++)
				{
					var section = chunkColumn.Sections[s];
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

		                    int[] indexes = new int[wordCount];
		                    for (int w = 0; w < wordCount; w++)
		                    {
			                    int word = defStream.ReadInt32();
			                    indexes[w] = word;
		                    }

		                    uint[] pallete = new uint[0];
		                    if (isRuntime)
		                    {
			                    int palleteSize = VarInt.ReadSInt32(stream);
			                    pallete = new uint[palleteSize];
			                    for (int pi = 0; pi < pallete.Length; pi++)
			                    {
				                    pallete[pi] = (uint) VarInt.ReadSInt32(stream);
			                    }
		                    }

		                    long afterPaletteIndex = defStream.BaseStream.Position;

		                    int position = 0;
		                    for (int w = indexes.Length-1; w > 0; w--)
		                    {
			                    int word = indexes[w];
			                    for (int block = 0; block < blocksPerWord; block++)
			                    {
				                    if (position >= 4096) continue; // padding bytes
				                    
				                    int state = (word >> ((position % blocksPerWord) * bitsPerBlock)) &
				                                ((1 << bitsPerBlock) - 1);
				                    int x = (position >> 8) & 0xF;
				                    int y = position & 0xF;
				                    int z = (position >> 4) & 0xF;

				                    if (storage == 0)
				                    {
					                    if (_blockStateMap.TryGetValue(pallete[state], out var bs))
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
							                    if (result.Data != 0)
							                    {
								                    //TODO: In order for this to work, we need to fix blockstate properties.

								                    int meta = (int) result.Data;
								                    switch (meta)
								                    {
									                    case 0:
										                    meta = 0;
										                    break;
									                    case 1:
										                    meta = 5;
										                    break;
									                    case 2:
										                    meta = 4;
										                    break;
									                    case 3:
										                    meta = 3;
										                    break;
									                    case 4:
										                    meta = 2;
										                    break;
									                    case 5:
										                    meta = 1;
										                    break;
								                    }
								                    Log.Warn($"METAAAA: " + meta);
								                    a = GetBlockStateFromRotationMeta(a, meta);
							                    }
							                    section.Set(15 - x, 15 - y, 15 - z, a);
						                    }
						                    else
						                    {
							                    section.Set(15 - x, 15 - y, 15 - z,
								                    BlockFactory.GetBlockState(bs.Name));
						                    }
					                    }
				                    }
				                    else
				                    {
					                    //TODO.
				                    }

				                    //section.Set(x, 15 - y, z, BlockFactory.GetBlockStateByRuntimeId(pallete[state]));

				                   position++;
			                    }
		                    }

		                    defStream.BaseStream.Position = afterPaletteIndex;
	                    }
                    }
                    else
					{
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
									int idx = (x << 8) + (z << 4) +y;

									var result = BlockFactory.RuntimeIdTable.Where(xx =>
										xx.Id == blockIds[idx] && xx.Data == data[idx]).ToArray();
									if (result.Length > 0)
									{
										//	result[0].sRuntimeId

										section.Set(x, y, z, BlockFactory.GetBlockState((uint)result[0].RuntimeId));
									}
									//else
									{

										var state = BlockFactory.GetBlockStateID(blockIds[idx], data[idx]);
									//	section.Set(x, y, z, BlockFactory.GetBlockState(state));
									}
								}
							}
						}
                    }

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
						Log.Debug($"x={x}, z={z}");
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
					Log.Warn($"Still have data to read\n{Packet.HexDump(defStream.ReadBytes((int)(stream.Length - stream.Position)))}");
				}

				//Done processing this chunk, send to world
                BaseClient.ChunkReceived(chunkColumn);
			}

			
		}
		
		private IBlockState GetBlockStateFromRotationMeta(IBlockState state, int meta)
		{
			var p = StateProperty.Parse("facing");
			
			switch(meta) {
				case 0:
					return state.WithProperty(p,"down");
					break;
				case 1:
					return state.WithProperty(p,"up");
					break;
				case 2:
					return state.WithProperty(p,"north");
					break;
				case 3:
					return state.WithProperty(p,"south");
					break;
				case 4:
					return state.WithProperty(p,"west");
					break;
				case 5:
					return state.WithProperty(p,"east");
					break;				
			
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
			UnhandledPackage(message);
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
			UnhandledPackage(message);
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

		public override void HandleMcpeMoveEntityDelta(McpeMoveEntityDelta message)
		{
			if (message.runtimeEntityId != Client.EntityId)
			{
				//Log.Info($"Prev: {message.prevSentPosition} New: {message.currentPosition}");
				if (message.prevSentPosition == null || message.currentPosition == null) return;
				
				BaseClient.WorldReceiver.UpdateEntityPosition(message.runtimeEntityId, new PlayerLocation(message.currentPosition),
					true);
			}
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
