using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Worlds.Generators;
using fNbt;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using MiNET.BlockEntities;
using MiNET.Utils;
using MiNET.Worlds;
using NLog;

namespace Alex.Worlds
{
	public class AnvilWorldProvider : IWorldGenerator
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AnvilWorldProvider));

		public static readonly Dictionary<int, Tuple<int, Func<int, byte, byte>>> Convert;

		public IWorldGenerator MissingChunkProvider { get; set; }

		public LevelInfo LevelInfo { get; private set; }

		public string BasePath { get; private set; }

		public Dimension Dimension { get; set; }

		public bool IsCaching { get; private set; } = true;

		public bool ReadSkyLight { get; set; } = true;

		public bool ReadBlockLight { get; set; } = true;

		public bool Locked { get; set; } = false;

		static AnvilWorldProvider()
		{
			var air = new Mapper(0, (i, b) => 0);

			Convert = new Dictionary<int, Tuple<int, Func<int, byte, byte>>>
			{
				/*{36, new NoDataMapper(250)}, // minecraft:piston_extension		=> MovingBlock
				{43, new Mapper(43, (i, b) => (byte) (b == 6 ? 7 : b == 7 ? 6 : b))}, // Fence		=> Fence
				{44, new Mapper(44, (i, b) => (byte) (b == 6 ? 7 : b == 7 ? 6 : b == 14 ? 15 : b == 15 ? 14 : b))}, // Fence		=> Fence
				{
					77, new Mapper(77, delegate(int i, byte b) // stone_button
					{
						switch (b & 0x7f)
						{
							case 0:
								return (byte) BlockFace.Down;
							case 1:
								return (byte) BlockFace.South;
							case 2:
								return (byte) BlockFace.North;
							case 3:
								return (byte) BlockFace.West;
							case 4:
								return (byte) BlockFace.East;
							case 5:
								return (byte) BlockFace.Up;
						}

						return 0;
					})
				},
				{84, new NoDataMapper(25)}, // minecraft:jukebox		=> noteblock
				{85, new Mapper(85, (i, b) => 0)}, // Fence		=> Fence
				{95, new NoDataMapper(241)}, // minecraft:stained_glass	=> Stained Glass
				{96, new Mapper(96, (i, b) => (byte) (((b & 0x04) << 1) | ((b & 0x08) >> 1) | (3 - (b & 0x03))))}, // Trapdoor Fix
				{125, new NoDataMapper(157)}, // minecraft:double_wooden_slab	=> minecraft:double_wooden_slab
				{126, new NoDataMapper(158)}, // minecraft:wooden_slab		=> minecraft:wooden_slab
				{
					143, new Mapper(143, delegate(int i, byte b) // wooden_button
					{
						switch (b & 0x7f)
						{
							case 0:
								return (byte) BlockFace.Down; // 0
							case 1:
								return (byte) BlockFace.South; // 5
							case 2:
								return (byte) BlockFace.North; // 4
							case 3:
								return (byte) BlockFace.West; // 3
							case 4:
								return (byte) BlockFace.East; // 2
							case 5:
								return (byte) BlockFace.Up; // 1
						}

						return 0;
					})
				},
				{157, new NoDataMapper(126)}, // minecraft:activator_rail
				{158, new NoDataMapper(125)}, // minecraft:dropper
				{166, new NoDataMapper(95)}, // minecraft:barrier		=> (Invisible Bedrock)
				{167, new Mapper(167, (i, b) => (byte) (((b & 0x04) << 1) | ((b & 0x08) >> 1) | (3 - (b & 0x03))))}, //Fix iron_trapdoor
				{176, air}, // minecraft:standing_banner		=> Air
				{177, air}, // minecraft:wall_banner		=> Air
				{188, new Mapper(85, (i, b) => 1)}, // Spruce Fence		=> Fence
				{189, new Mapper(85, (i, b) => 2)}, // Birch Fence		=> Fence
				{190, new Mapper(85, (i, b) => 3)}, // Jungle Fence		=> Fence
				{191, new Mapper(85, (i, b) => 5)}, // Dark Oak Fence	=> Fence
				{192, new Mapper(85, (i, b) => 4)}, // Acacia Fence		=> Fence
				{198, new NoDataMapper(208)}, // minecraft:end_rod	=> EndRod
				{199, new NoDataMapper(240)}, // minecraft:chorus_plant
				{202, new Mapper(201, (i, b) => 2)}, // minecraft:purpur_pillar => PurpurBlock:2 (idk why)
				{204, new Mapper(181, (i, b) => 1)}, // minecraft:purpur_double_slab
				{205, new Mapper(182, (i, b) => 1)}, // minecraft:purpur_slab
				{207, new NoDataMapper(244)}, // minecraft:beetroot_block
				{208, new NoDataMapper(198)}, // minecraft:grass_path
				{210, new NoDataMapper(188)}, // repeating_command_block
				{211, new NoDataMapper(189)}, // minecraft:chain_command_block
				{212, new NoDataMapper(297)}, // Frosted Ice
				{218, new NoDataMapper(251)}, // minecraft:observer => Observer
				{219, new Mapper(218, (i, b) => 0)}, // => minecraft:white_shulker_box
				{220, new Mapper(218, (i, b) => 1)}, // => minecraft:orange_shulker_box
				{221, new Mapper(218, (i, b) => 2)}, // => minecraft:magenta_shulker_box
				{222, new Mapper(218, (i, b) => 3)}, // => minecraft:light_blue_shulker_box 
				{223, new Mapper(218, (i, b) => 4)}, // => minecraft:yellow_shulker_box 
				{224, new Mapper(218, (i, b) => 5)}, // => minecraft:lime_shulker_box 
				{225, new Mapper(218, (i, b) => 6)}, // => minecraft:pink_shulker_box 
				{226, new Mapper(218, (i, b) => 7)}, // => minecraft:gray_shulker_box 
				{227, new Mapper(218, (i, b) => 8)}, // => minecraft:light_gray_shulker_box 
				{228, new Mapper(218, (i, b) => 9)}, // => minecraft:cyan_shulker_box 
				{229, new Mapper(218, (i, b) => 10)}, // => minecraft:purple_shulker_box 
				{230, new Mapper(218, (i, b) => 11)}, // => minecraft:blue_shulker_box 
				{231, new Mapper(218, (i, b) => 12)}, // => minecraft:brown_shulker_box 
				{232, new Mapper(218, (i, b) => 13)}, // => minecraft:green_shulker_box 
				{233, new Mapper(218, (i, b) => 14)}, // => minecraft:red_shulker_box 
				{234, new Mapper(218, (i, b) => 15)}, // => minecraft:black_shulker_box 

				{235, new NoDataMapper(220)}, // => minecraft:white_glazed_terracotta
				{236, new NoDataMapper(221)}, // => minecraft:orange_glazed_terracotta
				{237, new NoDataMapper(222)}, // => minecraft:magenta_glazed_terracotta
				{238, new NoDataMapper(223)}, // => minecraft:light_blue_glazed_terracotta
				{239, new NoDataMapper(224)}, // => minecraft:yellow_glazed_terracotta
				{240, new NoDataMapper(225)}, // => minecraft:lime_glazed_terracotta
				{241, new NoDataMapper(226)}, // => minecraft:pink_glazed_terracotta
				{242, new NoDataMapper(227)}, // => minecraft:gray_glazed_terracotta
				{243, new NoDataMapper(228)}, // => minecraft:light_gray_glazed_terracotta
				{244, new NoDataMapper(229)}, // => minecraft:cyan_glazed_terracotta
				{245, new NoDataMapper(219)}, // => minecraft:purple_glazed_terracotta
				{246, new NoDataMapper(231)}, // => minecraft:blue_glazed_terracotta
				{247, new NoDataMapper(232)}, // => minecraft:brown_glazed_terracotta
				{248, new NoDataMapper(233)}, // => minecraft:green_glazed_terracotta
				{249, new NoDataMapper(234)}, // => minecraft:red_glazed_terracotta
				{250, new NoDataMapper(235)}, // => minecraft:black_glazed_terracotta

				{251, new NoDataMapper(236)}, // => minecraft:concrete
				{252, new NoDataMapper(237)}, // => minecraft:concrete_powder
				*/
			};
		}

		public AnvilWorldProvider()
		{
		}

		public AnvilWorldProvider(string basePath) : this()
		{
			BasePath = basePath;
		}

		private bool _isInitialized = false;
		private object _initializeSync = new object();

		public void Initialize()
		{
			if (_isInitialized) return; // Quick exit

			lock (_initializeSync)
			{
				if (_isInitialized) return;

				BasePath = BasePath ?? Config.GetProperty("PCWorldFolder", "World").Trim();

				NbtFile file = new NbtFile();
				var levelFileName = Path.Combine(BasePath, "level.dat");
				if (File.Exists(levelFileName))
				{
					file.LoadFromFile(levelFileName);
					NbtTag dataTag = file.RootTag["Data"];
					LevelInfo = new LevelInfo(dataTag);
				}
				else
				{
					Log.Warn($"No level.dat found at {levelFileName}. Creating empty.");
					LevelInfo = new LevelInfo();
				}

				switch (Dimension)
				{
					case Dimension.Overworld:
						break;
					case Dimension.Nether:
						BasePath = Path.Combine(BasePath, @"DIM-1");
						break;
					case Dimension.TheEnd:
						BasePath = Path.Combine(BasePath, @"DIM1");
						break;
				}

				_isInitialized = true;
			}
		}
		
		public IChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			return GetChunk(chunkCoordinates, BasePath, MissingChunkProvider);
		}

	//	public Queue<Block> LightSources { get; set; } = new Queue<Block>();

		public IChunkColumn GetChunk(ChunkCoordinates coordinates, string basePath, IWorldGenerator generator)
		{
			try
			{
				const int width = 32;
				const int depth = 32;

				int rx = coordinates.X >> 5;
				int rz = coordinates.Z >> 5;

				string filePath = Path.Combine(basePath, string.Format(@"region{2}r.{0}.{1}.mca", rx, rz, Path.DirectorySeparatorChar));

				if (!File.Exists(filePath))
				{
					var chunkColumn = generator?.GenerateChunkColumn(coordinates);
					return chunkColumn;
				}

				using (var regionFile = File.OpenRead(filePath))
				{
					int locationIndex = ((coordinates.X & (width - 1)) + (coordinates.Z & (depth - 1)) * width) << 2;
					regionFile.Seek(locationIndex, SeekOrigin.Begin);

					byte[] offsetBuffer = new byte[4];
					regionFile.Read(offsetBuffer, 0, 3);
					Array.Reverse(offsetBuffer);
					int offset = BitConverter.ToInt32(offsetBuffer, 0) << 4;

					int sectorCount = regionFile.ReadByte();

					if (offset == 0 || sectorCount == 0)
					{
						var chunkColumn = generator?.GenerateChunkColumn(coordinates);
						return chunkColumn;
					}

					regionFile.Seek(offset + 4, SeekOrigin.Begin); //Offset + the length header

					int compressionMode = regionFile.ReadByte();

					if (compressionMode != 0x02)
						throw new Exception($"CX={coordinates.X}, CZ={coordinates.Z}, NBT wrong compression. Expected 0x02, got 0x{compressionMode:X2}. " +
											$"Offset={offset}, Sectors={sectorCount}");

					var nbt = new NbtFile();
					nbt.LoadFromStream(regionFile, NbtCompression.ZLib);

					int version = nbt.RootTag["DataVersion"].IntValue;
					NbtCompound dataTag = (NbtCompound)nbt.RootTag["Level"];

					bool isPocketEdition = false;
					if (dataTag.Contains("MCPE BID"))
					{
						isPocketEdition = dataTag["MCPE BID"].ByteValue == 1;
					}

					NbtList sections = dataTag["Sections"] as NbtList;

					Worlds.ChunkColumn chunk = new Worlds.ChunkColumn()
					{
						X = coordinates.X,
						Z = coordinates.Z
					};

					if (version >= 1477)
					{
						chunk.BiomeId = dataTag["Biomes"].IntArrayValue;
					}
					else
					{
						var byteArray = dataTag["Biomes"].ByteArrayValue;
						chunk.BiomeId = new int[byteArray.Length];
						for (int i = 0; i < byteArray.Length; i++)
						{
							chunk.BiomeId[i] = byteArray[i];
						}
					}

					//chunk.b

					//if (chunk.biomeId.Length > 256) throw new Exception();

					NbtTag heights = dataTag["HeightMap"] as NbtIntArray;
					if (heights != null)
					{
						int[] intHeights = heights.IntArrayValue;
						for (int i = 0; i < 256; i++)
						{
							chunk.Height[i] = (byte)intHeights[i];
						}
					}

					// This will turn into a full chunk column
					foreach (NbtTag sectionTag in sections)
					{
						if (version >= 1477)
						{
							ReadSection(sectionTag, chunk, !isPocketEdition);
						}
						else
						{
							ReadOldSection(sectionTag, chunk, !isPocketEdition);
						}
					}

					NbtList entities = dataTag["Entities"] as NbtList;
					if (entities != null)
					{
						chunk.Entities = entities.ToArray<NbtCompound>();
					}

					/*NbtList blockEntities = dataTag["TileEntities"] as NbtList;
					if (blockEntities != null)
					{
						foreach (var nbtTag in blockEntities)
						{
							var blockEntityTag = (NbtCompound)nbtTag.Clone();
							string entityId = blockEntityTag["id"].StringValue;
							int x = blockEntityTag["x"].IntValue;
							int y = blockEntityTag["y"].IntValue;
							int z = blockEntityTag["z"].IntValue;

							if (entityId.StartsWith("minecraft:"))
							{
								var id = entityId.Split(':')[1];

								entityId = id.First().ToString().ToUpper() + id.Substring(1);

								blockEntityTag["id"] = new NbtString("id", entityId);
							}

							BlockEntity blockEntity = BlockEntityFactory.GetBlockEntityById(entityId);

							if (blockEntity != null)
							{
								blockEntityTag.Name = string.Empty;

								if (blockEntity is Sign)
								{
									// Remove the JSON stuff and get the text out of extra data.
									// TAG_String("Text2"): "{"extra":["10c a loaf!"],"text":""}"
									CleanSignText(blockEntityTag, "Text1");
									CleanSignText(blockEntityTag, "Text2");
									CleanSignText(blockEntityTag, "Text3");
									CleanSignText(blockEntityTag, "Text4");
								}
								else if (blockEntity is ChestBlockEntity)
								{
									NbtList items = (NbtList)blockEntityTag["Items"];

									if (items != null)
									{
										//for (byte i = 0; i < items.Count; i++)
										//{
										//	NbtCompound item = (NbtCompound) items[i];

										//	item.Add(new NbtShort("OriginalDamage", item["Damage"].ShortValue));

										//	byte metadata = (byte) (item["Damage"].ShortValue & 0xff);
										//	item.Remove("Damage");
										//	item.Add(new NbtByte("Damage", metadata));
										//}
									}
								}

								//chunk.SetBlockEntity(new BlockCoordinates(x, y, z), blockEntityTag);
							}
							else
							{
								
							}
						}
					}*/

					//NbtList tileTicks = dataTag["TileTicks"] as NbtList;

					//chunk.RecalcHeight();

				//	chunk.isDirty = false;
				//	chunk.NeedSave = false;

					//if (Config.GetProperty("CalculateLights", false))
					{
					//	SkyLightBlockAccess blockAccess = new SkyLightBlockAccess(this, chunk);
				//		new SkyLightCalculations().RecalcSkyLight(chunk, blockAccess);
						//TODO: Block lights.
					}

					chunk.CalculateHeight();
					return chunk;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Loading chunk {coordinates}", e);
				var chunkColumn = generator?.GenerateChunkColumn(coordinates);
				if (chunkColumn != null)
				{
					//chunkColumn.NeedSave = true;
				}

				return chunkColumn;
			}
		}

		private void ReadSection(NbtTag sectionTag, Worlds.ChunkColumn chunk, bool convertBid = true)
		{
			int sectionIndex = sectionTag["Y"].ByteValue;
			NbtList palette = sectionTag["Palette"] as NbtList;
			long[] blockStates = sectionTag["BlockStates"].LongArrayValue;

			byte[] blockLight = sectionTag["BlockLight"].ByteArrayValue;
			byte[] skyLight = sectionTag["SkyLight"].ByteArrayValue;

			var section = new ExtendedBlockStorage(sectionIndex, ReadSkyLight);
			section.GetData().SetDataFromNbt(palette, blockStates);

			if (ReadSkyLight)
			{
				section.SetSkylightArray(new NibbleArray()
				{
					Data = skyLight
				});
			}

			if (ReadBlockLight)
			{
				section.SetBlocklightArray(new NibbleArray()
				{
					Data = blockLight
				});
			}

			section.RemoveInvalidBlocks();

			chunk.Chunks[sectionIndex] = section;
		}

		private void ReadOldSection(NbtTag sectionTag, Worlds.ChunkColumn chunk, bool convertBid = true)
		{
			int sectionIndex = sectionTag["Y"].ByteValue;
			byte[] blocks = sectionTag["Blocks"].ByteArrayValue;
			byte[] data = sectionTag["Data"].ByteArrayValue;
			NbtTag addTag = sectionTag["Add"];
			byte[] adddata = new byte[2048];
			if (addTag != null) adddata = addTag.ByteArrayValue;
			byte[] blockLight = sectionTag["BlockLight"].ByteArrayValue;
			byte[] skyLight = sectionTag["SkyLight"].ByteArrayValue;

			var section = new ExtendedBlockStorage(sectionIndex, ReadSkyLight);

			section.GetData().SetDataFromNbt(blocks, data, adddata);

			if (ReadSkyLight)
			{
				section.SetSkylightArray(new NibbleArray()
				{
					Data = skyLight
				});
			}

			if (ReadBlockLight)
			{
				section.SetBlocklightArray(new NibbleArray()
				{
					Data = blockLight
				});
			}

			section.RemoveInvalidBlocks();

			chunk.Chunks[sectionIndex] = section;
		}

		private static void SetNibble4(byte[] arr, int index, byte value)
		{
			value &= 0xF;
			var idx = index >> 1;
			arr[idx] &= (byte)(0xF << (((index + 1) & 1) << 2));
			arr[idx] |= (byte)(value << ((index & 1) << 2));
		}

		public Vector3 GetSpawnPoint()
		{
			var spawnPoint = new Vector3(LevelInfo.SpawnX, LevelInfo.SpawnY + 2 /* + WaterOffsetY*/, LevelInfo.SpawnZ);
			if (Dimension == Dimension.TheEnd)
			{
				spawnPoint = new Vector3(100, 49, 0);
			}
			else if (Dimension == Dimension.Nether)
			{
				spawnPoint = new Vector3(0, 80, 0);
			}

			if (spawnPoint.Y > 256) spawnPoint.Y = 255;

			return spawnPoint;
		}

		public long GetTime()
		{
			return LevelInfo.Time;
		}

		public string GetName()
		{
			return LevelInfo.LevelName;
		}

		public void SaveLevelInfo(LevelInfo level)
		{
			if (!Directory.Exists(BasePath))
				Directory.CreateDirectory(BasePath);
			else
				return;

			if (LevelInfo.SpawnY <= 0) LevelInfo.SpawnY = 256;

			NbtFile file = new NbtFile();
			NbtTag dataTag = file.RootTag["Data"] = new NbtCompound("Data");
			level.SaveToNbt(dataTag);
			file.SaveToFile(Path.Combine(BasePath, "level.dat"), NbtCompression.ZLib);
		}
		public bool HaveNether()
		{
			return Directory.Exists(Path.Combine(BasePath, @"DIM-1"));
		}

		public bool HaveTheEnd()
		{
			return Directory.Exists(Path.Combine(BasePath, @"DIM1"));
		}

		public static void SaveChunk(MiNET.Worlds.ChunkColumn chunk, string basePath)
		{
			// WARNING: This method does not consider growing size of the chunks. Needs refactoring to find
			// free sectors and clear up old ones. It works fine as long as no dynamic data is written
			// like block entity data (signs etc).

			Stopwatch time = new Stopwatch();
			time.Restart();

			chunk.NeedSave = false;

			var coordinates = new ChunkCoordinates(chunk.x, chunk.z);

			int width = 32;
			int depth = 32;

			int rx = coordinates.X >> 5;
			int rz = coordinates.Z >> 5;

			string filePath = Path.Combine(basePath, string.Format(@"region{2}r.{0}.{1}.mca", rx, rz, Path.DirectorySeparatorChar));

			Log.Debug($"Save chunk X={chunk.x}, Z={chunk.z} to {filePath}");

			if (!File.Exists(filePath))
			{
				// Make sure directory exist
				Directory.CreateDirectory(Path.Combine(basePath, "region"));

				// Create empty region file
				using (var regionFile = File.Open(filePath, FileMode.CreateNew))
				{
					byte[] buffer = new byte[8192];
					regionFile.Write(buffer, 0, buffer.Length);
				}
			}

			Stopwatch testTime = new Stopwatch();

			using (var regionFile = File.Open(filePath, FileMode.Open))
			{
				int locationIndex = ((coordinates.X & (width - 1)) + (coordinates.Z & (depth - 1)) * width) << 2;
				regionFile.Seek(locationIndex, SeekOrigin.Begin);

				byte[] offsetBuffer = new byte[4];
				regionFile.Read(offsetBuffer, 0, 3);
				Array.Reverse(offsetBuffer);
				int offset = BitConverter.ToInt32(offsetBuffer, 0) << 4;

				int sectorCount = regionFile.ReadByte();

				testTime.Restart(); // RESTART

				// Seriaize NBT to get lenght
				NbtFile nbt = CreateNbtFromChunkColumn(chunk);

				testTime.Stop();

				byte[] nbtBuf = nbt.SaveToBuffer(NbtCompression.ZLib);
				int nbtLength = nbtBuf.Length;
				byte nbtSectorCount = (byte)Math.Ceiling(nbtLength / 4096d);

				// Don't write yet, just use the lenght

				//TODO: Search for available sectors
				if (offset == 0 || sectorCount == 0 || nbtSectorCount > sectorCount)
				{
					if (Log.IsDebugEnabled) if (sectorCount != 0) Log.Warn($"Creating new sectors for this chunk even tho it existed. Old sector count={sectorCount}, new sector count={nbtSectorCount} (lenght={nbtLength})");

					regionFile.Seek(0, SeekOrigin.End);
					offset = (int)((int)regionFile.Position & 0xfffffff0);

					regionFile.Seek(locationIndex, SeekOrigin.Begin);

					byte[] bytes = BitConverter.GetBytes(offset >> 4);
					Array.Reverse(bytes);
					regionFile.Write(bytes, 0, 3);
					regionFile.WriteByte(nbtSectorCount);

					regionFile.Seek(4096 + locationIndex, SeekOrigin.Begin);
					bytes = BitConverter.GetBytes((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
					Array.Reverse(bytes);
					regionFile.Write(bytes, 0, bytes.Length);
				}

				byte[] lenghtBytes = BitConverter.GetBytes(nbtLength + 1);
				Array.Reverse(lenghtBytes);

				regionFile.Seek(offset, SeekOrigin.Begin);
				regionFile.Write(lenghtBytes, 0, 4); // Lenght
				regionFile.WriteByte(0x02); // Compression mode

				regionFile.Write(nbtBuf, 0, nbtBuf.Length);

				int reminder;
				Math.DivRem(nbtLength + 4, 4096, out reminder);

				byte[] padding = new byte[4096 - reminder];
				if (padding.Length > 0) regionFile.Write(padding, 0, padding.Length);

				testTime.Stop(); // STOP

				Log.Warn($"Took {time.ElapsedMilliseconds}ms to save. And {testTime.ElapsedMilliseconds}ms to generate bytes from NBT");
			}
		}

		public static NbtFile CreateNbtFromChunkColumn(MiNET.Worlds.ChunkColumn chunk)
		{
			var nbt = new NbtFile();

			NbtCompound levelTag = new NbtCompound("Level");
			nbt.RootTag.Add(levelTag);

			levelTag.Add(new NbtByte("MCPE BID", 1)); // Indicate that the chunks contain PE block ID's.

			levelTag.Add(new NbtInt("xPos", chunk.x));
			levelTag.Add(new NbtInt("zPos", chunk.z));
			levelTag.Add(new NbtByteArray("Biomes", chunk.biomeId));

			NbtList sectionsTag = new NbtList("Sections", NbtTagType.Compound);
			levelTag.Add(sectionsTag);

			for (int i = 0; i < 16; i++)
			{
				var section = chunk.chunks[i];
				if (section.IsAllAir()) continue;

				NbtCompound sectionTag = new NbtCompound();
				sectionsTag.Add(sectionTag);
				sectionTag.Add(new NbtByte("Y", (byte)i));

				byte[] blocks = new byte[4096];
				byte[] data = new byte[2048];
				byte[] blockLight = new byte[2048];
				byte[] skyLight = new byte[2048];

				{
					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int y = 0; y < 16; y++)
							{
								int anvilIndex = y * 16 * 16 + z * 16 + x;
								byte blockId = section.GetBlock(x, y, z);
								blocks[anvilIndex] = blockId;
								SetNibble4(data, anvilIndex, section.GetMetadata(x, y, z));
								SetNibble4(blockLight, anvilIndex, section.GetBlocklight(x, y, z));
								SetNibble4(skyLight, anvilIndex, section.GetSkylight(x, y, z));
							}
						}
					}
				}
				sectionTag.Add(new NbtByteArray("Blocks", blocks));
				sectionTag.Add(new NbtByteArray("Data", data));
				sectionTag.Add(new NbtByteArray("BlockLight", blockLight));
				sectionTag.Add(new NbtByteArray("SkyLight", skyLight));
			}

			int[] heights = new int[256];
			for (int h = 0; h < heights.Length; h++)
			{
				heights[h] = chunk.height[h];
			}
			levelTag.Add(new NbtIntArray("HeightMap", heights));

			// TODO: Save entities
			NbtList entitiesTag = new NbtList("Entities", NbtTagType.Compound);
			//foreach(var entity in )
			levelTag.Add(entitiesTag);

			NbtList blockEntitiesTag = new NbtList("TileEntities", NbtTagType.Compound);
			foreach (NbtCompound blockEntityNbt in chunk.BlockEntities.Values)
			{
				NbtCompound nbtClone = (NbtCompound)blockEntityNbt.Clone();
				nbtClone.Name = null;
				blockEntitiesTag.Add(nbtClone);
			}

			levelTag.Add(blockEntitiesTag);

			levelTag.Add(new NbtList("TileTicks", NbtTagType.Compound));

			return nbt;
		}
	}
}
