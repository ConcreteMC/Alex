using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Utils;
using Alex.Worlds.Generators;
using AutoMapper;
using fNbt;
using fNbt.Tags;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Worlds
{
	public class Mapper : Tuple<int, Func<int, byte, byte>>
	{
		public Mapper(int blockId, Func<int, byte, byte> dataMapper)
			: base(blockId, dataMapper)
		{
		}
	}

	public class NoDataMapper : Mapper
	{
		public NoDataMapper(int blockId) : base(blockId, (bi, i1) => i1)
		{
		}
	}

	public class AnvilWorldProvider : IWorldGenerator
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(AnvilWorldProvider));

		public IWorldGenerator MissingChunkProvider { get; set; }
		public LevelInfo LevelInfo { get; private set; }

		public string BasePath { get; private set; }

		public bool IsCaching { get; private set; } = true;

		public bool ReadSkyLight { get; set; } = true;

		public bool ReadBlockLight { get; set; } = true;

		public bool Locked { get; set; } = false;

		static AnvilWorldProvider()
		{
			if (_1p12to1p13.Count == 0)
			{
				LoadBlockConverter();
			}
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
					
				}

				_isInitialized = true;
			}
		}

		public LevelInfo GetInfo()
		{
			return LevelInfo;
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

					//int version = nbt.RootTag["DataVersion"].IntValue;
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

					bool isNewFormat = dataTag.Contains("Status");

				//	if (version >= 1477)
					if (isNewFormat)
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


					if (chunk.BiomeId.Length != 256)
					{
						Array.Resize(ref chunk.BiomeId, 256);
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
						//if (version >= 1477)
						if (isNewFormat)
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
				Log.Error(e, $"Loading chunk {coordinates}");
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
			section.Data.SetDataFromNbt(palette, blockStates);

			if (ReadSkyLight)
			{
				section.SkyLight = new NibbleArray()
				{
					Data = skyLight
				};
			}

			if (ReadBlockLight)
			{
				section.BlockLight = new NibbleArray()
				{
					Data = blockLight
				};
			}

			section.RemoveInvalidBlocks();

			chunk.Chunks[sectionIndex] = section;
		}
		private static byte Nibble4(byte[] arr, int index)
		{
			return (byte)(arr[index >> 1] >> ((index & 1) << 2) & 0xF);
		}

		private void ReadOldSection(NbtTag sectionTag, Worlds.ChunkColumn chunk, bool convertBid = true)
		{
			//throw new NotImplementedException("TODO: Implement a id:meta to blockstate converter.");

			int sectionIndex = sectionTag["Y"].ByteValue;
			byte[] blocks = sectionTag["Blocks"].ByteArrayValue;
			byte[] data = sectionTag["Data"].ByteArrayValue;
			NbtTag addTag = sectionTag["Add"];
			byte[] adddata = new byte[2048];
			if (addTag != null) adddata = addTag.ByteArrayValue;
			byte[] blockLight = sectionTag["BlockLight"].ByteArrayValue;
			byte[] skyLight = sectionTag["SkyLight"].ByteArrayValue;

			var section = new ExtendedBlockStorage(sectionIndex, ReadSkyLight);

			for (int i = 0; i < 4096; i++)
			{
				int blockIdExtensionData = adddata == null ? 0 : Nibble4(adddata, i); //.Get(j, k, l);
				uint blockStateId = (uint)((blockIdExtensionData << 12) | ((blocks[i]) << 4) | Nibble4(data, i)); //.Get(j, k, l);

				if (_1p12to1p13.TryGetValue(blockStateId, out blockStateId))
				{
					section.Data.Set(i, BlockFactory.GetBlockState(blockStateId));
				}

				//section.GetData().Set(i, BlockFactory.GetBlockState(blockStateId));
			}

		//	section.GetData().SetDataFromNbt(blocks, data, adddata);

			if (ReadSkyLight)
			{
				section.SkyLight = new NibbleArray()
				{
					Data = skyLight
				};
			}

			if (ReadBlockLight)
			{
				section.BlockLight = new NibbleArray()
				{
					Data = blockLight
				};
			}

			section.RemoveInvalidBlocks();

			chunk.Chunks[sectionIndex] = section;
		}

		public Vector3 GetSpawnPoint()
		{
			var spawnPoint = new Vector3(LevelInfo.SpawnX, LevelInfo.SpawnY + 2 /* + WaterOffsetY*/, LevelInfo.SpawnZ);

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

		public bool HaveNether()
		{
			return Directory.Exists(Path.Combine(BasePath, @"DIM-1"));
		}

		public bool HaveTheEnd()
		{
			return Directory.Exists(Path.Combine(BasePath, @"DIM1"));
		}

		private static uint GetBlockStateId(string name)
		{
			var res = BlockFactory.GetBlockState(name);
			if (res == null) return 0;
			return res.ID;
		}

		private static Dictionary<uint, uint> _1p12to1p13 = new Dictionary<uint, uint>();
		private static void LoadBlockConverter()
		{
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(0, 0), GetBlockStateId("minecraft:air"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 0), GetBlockStateId("minecraft:stone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 1), GetBlockStateId("minecraft:granite"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 2), GetBlockStateId("minecraft:polished_granite"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 3), GetBlockStateId("minecraft:diorite"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 4), GetBlockStateId("minecraft:polished_diorite"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 5), GetBlockStateId("minecraft:andesite"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(1, 6), GetBlockStateId("minecraft:polished_andesite"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(2, 0), GetBlockStateId("minecraft:grass_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(3, 0), GetBlockStateId("minecraft:dirt"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(3, 1), GetBlockStateId("minecraft:coarse_dirt"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(3, 2), GetBlockStateId("minecraft:podzol"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(4, 0), GetBlockStateId("minecraft:cobblestone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(5, 0), GetBlockStateId("minecraft:oak_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(5, 1), GetBlockStateId("minecraft:spruce_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(5, 2), GetBlockStateId("minecraft:birch_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(5, 3), GetBlockStateId("minecraft:jungle_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(5, 4), GetBlockStateId("minecraft:acacia_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(5, 5), GetBlockStateId("minecraft:dark_oak_planks"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:oak_sapling"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:spruce_sapling"));
			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:birch_sapling"));
			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:jungle_sapling"));
			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:acacia_sapling"));
			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(6, Índice_Data), GetBlockStateId("minecraft:dark_oak_sapling"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(7, 0), GetBlockStateId("minecraft:bedrock"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(8, 0), GetBlockStateId("minecraft:flowing_water"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(9, 0), GetBlockStateId("minecraft:water"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(10, 0), GetBlockStateId("minecraft:flowing_lava"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(11, 0), GetBlockStateId("minecraft:lava"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(12, 0), GetBlockStateId("minecraft:sand"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(12, 1), GetBlockStateId("minecraft:red_sand"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(13, 0), GetBlockStateId("minecraft:gravel"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(14, 0), GetBlockStateId("minecraft:gold_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(15, 0), GetBlockStateId("minecraft:iron_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(16, 0), GetBlockStateId("minecraft:coal_ore"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:oak_log"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:spruce_log"));
			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:birch_log"));
			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(17, Índice_Data), GetBlockStateId("minecraft:jungle_log"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:oak_leaves"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:spruce_leaves"));
			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:birch_leaves"));
			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 4) _1p12to1p13.Add(BlockFactory.GetBlockStateID(18, Índice_Data), GetBlockStateId("minecraft:jungle_leaves"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(19, 0), GetBlockStateId("minecraft:sponge"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(19, 1), GetBlockStateId("minecraft:wet_sponge"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(20, 0), GetBlockStateId("minecraft:glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(21, 0), GetBlockStateId("minecraft:lapis_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(22, 0), GetBlockStateId("minecraft:lapis_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(23, 0), GetBlockStateId("minecraft:dispenser"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(24, 0), GetBlockStateId("minecraft:sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(24, 1), GetBlockStateId("minecraft:chiseled_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(24, 2), GetBlockStateId("minecraft:cut_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(25, 0), GetBlockStateId("minecraft:note_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(26, 0), GetBlockStateId("minecraft:red_bed"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(27, 0), GetBlockStateId("minecraft:powered_rail"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(28, 0), GetBlockStateId("minecraft:detector_rail"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(29, 0), GetBlockStateId("minecraft:sticky_piston"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(30, 0), GetBlockStateId("minecraft:cobweb"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(31, 0), GetBlockStateId("minecraft:grass")); // dead_bush
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(31, 1), GetBlockStateId("minecraft:grass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(31, 2), GetBlockStateId("minecraft:fern"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(32, 0), GetBlockStateId("minecraft:dead_bush"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(33, 0), GetBlockStateId("minecraft:piston"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(34, 0), GetBlockStateId("minecraft:piston_head"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 0), GetBlockStateId("minecraft:white_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 1), GetBlockStateId("minecraft:orange_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 2), GetBlockStateId("minecraft:magenta_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 3), GetBlockStateId("minecraft:light_blue_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 4), GetBlockStateId("minecraft:yellow_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 5), GetBlockStateId("minecraft:lime_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 6), GetBlockStateId("minecraft:pink_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 7), GetBlockStateId("minecraft:gray_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 8), GetBlockStateId("minecraft:light_gray_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 9), GetBlockStateId("minecraft:cyan_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 10), GetBlockStateId("minecraft:purple_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 11), GetBlockStateId("minecraft:blue_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 12), GetBlockStateId("minecraft:brown_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 13), GetBlockStateId("minecraft:green_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 14), GetBlockStateId("minecraft:red_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(35, 15), GetBlockStateId("minecraft:black_wool"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(37, 0), GetBlockStateId("minecraft:dandelion"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 0), GetBlockStateId("minecraft:poppy"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 1), GetBlockStateId("minecraft:blue_orchid"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 2), GetBlockStateId("minecraft:allium"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 3), GetBlockStateId("minecraft:azure_bluet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 4), GetBlockStateId("minecraft:red_tulip"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 5), GetBlockStateId("minecraft:orange_tulip"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 6), GetBlockStateId("minecraft:white_tulip"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 7), GetBlockStateId("minecraft:pink_tulip"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(38, 8), GetBlockStateId("minecraft:oxeye_daisy"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(39, 0), GetBlockStateId("minecraft:brown_mushroom"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(40, 0), GetBlockStateId("minecraft:red_mushroom"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(41, 0), GetBlockStateId("minecraft:gold_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(42, 0), GetBlockStateId("minecraft:iron_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 0), GetBlockStateId("minecraft:oak_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 1), GetBlockStateId("minecraft:sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 2), GetBlockStateId("minecraft:oak_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 3), GetBlockStateId("minecraft:cobblestone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 4), GetBlockStateId("minecraft:bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 5), GetBlockStateId("minecraft:stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 6), GetBlockStateId("minecraft:quartz_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 7), GetBlockStateId("minecraft:nether_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 8), GetBlockStateId("minecraft:smooth_stone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 9), GetBlockStateId("minecraft:smooth_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(43, 14), GetBlockStateId("minecraft:smooth_quartz"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:stone_slab"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:sandstone_slab"));
			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:petrified_oak_slab"));
			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:cobblestone_slab"));
			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:brick_slab"));
			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:stone_brick_slab"));
			for (byte Índice_Data = 6; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:quartz_slab"));
			for (byte Índice_Data = 7; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(44, Índice_Data), GetBlockStateId("minecraft:nether_brick_slab"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(45, 0), GetBlockStateId("minecraft:bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(46, 0), GetBlockStateId("minecraft:tnt"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(47, 0), GetBlockStateId("minecraft:bookshelf"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(48, 0), GetBlockStateId("minecraft:mossy_cobblestone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(49, 0), GetBlockStateId("minecraft:obsidian"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(50, 0), GetBlockStateId("minecraft:torch"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(51, 0), GetBlockStateId("minecraft:fire"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(52, 0), GetBlockStateId("minecraft:mob_spawner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(53, 0), GetBlockStateId("minecraft:oak_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(54, 0), GetBlockStateId("minecraft:chest"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(55, 0), GetBlockStateId("minecraft:redstone_wire"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(56, 0), GetBlockStateId("minecraft:diamond_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(57, 0), GetBlockStateId("minecraft:diamond_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(58, 0), GetBlockStateId("minecraft:crafting_table"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(59, 0), GetBlockStateId("minecraft:wheat"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(60, 0), GetBlockStateId("minecraft:farmland"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(61, 0), GetBlockStateId("minecraft:furnace"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(62, 0), GetBlockStateId("minecraft:furnace"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(63, 0), GetBlockStateId("minecraft:sign"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(64, 0), GetBlockStateId("minecraft:oak_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(65, 0), GetBlockStateId("minecraft:ladder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(66, 0), GetBlockStateId("minecraft:rail"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(67, 0), GetBlockStateId("minecraft:cobblestone_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(68, 0), GetBlockStateId("minecraft:sign"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(69, 0), GetBlockStateId("minecraft:lever"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(70, 0), GetBlockStateId("minecraft:stone_pressure_plate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(71, 0), GetBlockStateId("minecraft:iron_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(72, 0), GetBlockStateId("minecraft:oak_pressure_plate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(73, 0), GetBlockStateId("minecraft:redstone_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(74, 0), GetBlockStateId("minecraft:redstone_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(75, 0), GetBlockStateId("minecraft:redstone_torch"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(76, 0), GetBlockStateId("minecraft:redstone_torch"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(77, 0), GetBlockStateId("minecraft:stone_button"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(78, 0), GetBlockStateId("minecraft:snow"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(79, 0), GetBlockStateId("minecraft:ice"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(80, 0), GetBlockStateId("minecraft:snow_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(81, 0), GetBlockStateId("minecraft:cactus"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(82, 0), GetBlockStateId("minecraft:clay"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(83, 0), GetBlockStateId("minecraft:sugar_cane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(84, 0), GetBlockStateId("minecraft:jukebox"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(85, 0), GetBlockStateId("minecraft:oak_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(86, 0), GetBlockStateId("minecraft:carved_pumpkin"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(87, 0), GetBlockStateId("minecraft:netherrack"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(88, 0), GetBlockStateId("minecraft:soul_sand"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(89, 0), GetBlockStateId("minecraft:glowstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(90, 0), GetBlockStateId("minecraft:portal"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(91, 0), GetBlockStateId("minecraft:jack_o_lantern"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(92, 0), GetBlockStateId("minecraft:cake"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(93, 0), GetBlockStateId("minecraft:repeater"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(94, 0), GetBlockStateId("minecraft:repeater"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 0), GetBlockStateId("minecraft:white_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 1), GetBlockStateId("minecraft:orange_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 2), GetBlockStateId("minecraft:magenta_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 3), GetBlockStateId("minecraft:light_blue_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 4), GetBlockStateId("minecraft:yellow_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 5), GetBlockStateId("minecraft:lime_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 6), GetBlockStateId("minecraft:pink_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 7), GetBlockStateId("minecraft:gray_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 8), GetBlockStateId("minecraft:light_gray_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 9), GetBlockStateId("minecraft:cyan_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 10), GetBlockStateId("minecraft:purple_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 11), GetBlockStateId("minecraft:blue_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 12), GetBlockStateId("minecraft:brown_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 13), GetBlockStateId("minecraft:green_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 14), GetBlockStateId("minecraft:red_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(95, 15), GetBlockStateId("minecraft:black_stained_glass"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(96, 0), GetBlockStateId("minecraft:oak_trapdoor"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(97, 0), GetBlockStateId("minecraft:infested_stone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(97, 1), GetBlockStateId("minecraft:infested_cobblestone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(97, 2), GetBlockStateId("minecraft:infested_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(97, 3), GetBlockStateId("minecraft:infested_mossy_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(97, 4), GetBlockStateId("minecraft:infested_cracked_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(97, 5), GetBlockStateId("minecraft:infested_chiseled_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(98, 0), GetBlockStateId("minecraft:stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(98, 1), GetBlockStateId("minecraft:mossy_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(98, 3), GetBlockStateId("minecraft:chiseled_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(98, 4), GetBlockStateId("minecraft:cracked_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(99, 0), GetBlockStateId("minecraft:brown_mushroom_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(100, 0), GetBlockStateId("minecraft:red_mushroom_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(101, 0), GetBlockStateId("minecraft:iron_bars"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(102, 0), GetBlockStateId("minecraft:glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(103, 0), GetBlockStateId("minecraft:melon_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(104, 0), GetBlockStateId("minecraft:pumpkin_stem"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(105, 0), GetBlockStateId("minecraft:melon_stem"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(106, 0), GetBlockStateId("minecraft:vine"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(107, 0), GetBlockStateId("minecraft:oak_fence_gate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(108, 0), GetBlockStateId("minecraft:brick_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(109, 0), GetBlockStateId("minecraft:stone_brick_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(110, 0), GetBlockStateId("minecraft:mycelium"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(111, 0), GetBlockStateId("minecraft:lily_pad"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(112, 0), GetBlockStateId("minecraft:nether_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(113, 0), GetBlockStateId("minecraft:nether_brick_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(114, 0), GetBlockStateId("minecraft:nether_brick_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(115, 0), GetBlockStateId("minecraft:nether_wart"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(116, 0), GetBlockStateId("minecraft:enchanting_table"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(117, 0), GetBlockStateId("minecraft:brewing_stand"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(118, 0), GetBlockStateId("minecraft:cauldron"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(119, 0), GetBlockStateId("minecraft:end_portal"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(120, 0), GetBlockStateId("minecraft:end_portal_frame"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(121, 0), GetBlockStateId("minecraft:end_stone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(122, 0), GetBlockStateId("minecraft:dragon_egg"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(123, 0), GetBlockStateId("minecraft:redstone_lamp"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(124, 0), GetBlockStateId("minecraft:redstone_lamp"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(125, 0), GetBlockStateId("minecraft:oak_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(125, 1), GetBlockStateId("minecraft:spruce_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(125, 2), GetBlockStateId("minecraft:birch_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(125, 3), GetBlockStateId("minecraft:jungle_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(125, 4), GetBlockStateId("minecraft:acacia_planks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(125, 5), GetBlockStateId("minecraft:dark_oak_planks"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:oak_slab"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:spruce_slab"));
			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:birch_slab"));
			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:jungle_slab"));
			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:acacia_slab"));
			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(126, Índice_Data), GetBlockStateId("minecraft:dark_oak_slab"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(127, 0), GetBlockStateId("minecraft:cocoa"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(128, 0), GetBlockStateId("minecraft:sandstone_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(129, 0), GetBlockStateId("minecraft:emerald_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(130, 0), GetBlockStateId("minecraft:ender_chest"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(131, 0), GetBlockStateId("minecraft:tripwire_hook"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(132, 0), GetBlockStateId("minecraft:tripwire"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(133, 0), GetBlockStateId("minecraft:emerald_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(134, 0), GetBlockStateId("minecraft:spruce_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(135, 0), GetBlockStateId("minecraft:birch_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(136, 0), GetBlockStateId("minecraft:jungle_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(137, 0), GetBlockStateId("minecraft:command_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(138, 0), GetBlockStateId("minecraft:beacon"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(139, 0), GetBlockStateId("minecraft:cobblestone_wall"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(139, 1), GetBlockStateId("minecraft:mossy_cobblestone_wall"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(140, 0), GetBlockStateId("minecraft:flower_pot"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(141, 0), GetBlockStateId("minecraft:carrots"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(142, 0), GetBlockStateId("minecraft:potatoes"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(143, 0), GetBlockStateId("minecraft:oak_button"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(144, 0), GetBlockStateId("minecraft:skeleton_skull"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(144, 1), GetBlockStateId("minecraft:wither_skeleton_skull"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(144, 2), GetBlockStateId("minecraft:zombie_head"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(144, 3), GetBlockStateId("minecraft:player_head"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(144, 4), GetBlockStateId("minecraft:creeper_head"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(144, 5), GetBlockStateId("minecraft:dragon_head"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(145, 0), GetBlockStateId("minecraft:anvil"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(145, 1), GetBlockStateId("minecraft:chipped_anvil"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(145, 2), GetBlockStateId("minecraft:damaged_anvil"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(146, 0), GetBlockStateId("minecraft:trapped_chest"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(147, 0), GetBlockStateId("minecraft:light_weighted_pressure_plate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(148, 0), GetBlockStateId("minecraft:heavy_weighted_pressure_plate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(149, 0), GetBlockStateId("minecraft:comparator"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(150, 0), GetBlockStateId("minecraft:comparator"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(151, 0), GetBlockStateId("minecraft:daylight_detector"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(152, 0), GetBlockStateId("minecraft:redstone_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(153, 0), GetBlockStateId("minecraft:nether_quartz_ore"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(154, 0), GetBlockStateId("minecraft:hopper"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(155, 0), GetBlockStateId("minecraft:quartz_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(155, 1), GetBlockStateId("minecraft:chiseled_quartz_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(155, 2), GetBlockStateId("minecraft:quartz_pillar"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(156, 0), GetBlockStateId("minecraft:quartz_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(157, 0), GetBlockStateId("minecraft:activator_rail"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(158, 0), GetBlockStateId("minecraft:dropper"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 0), GetBlockStateId("minecraft:white_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 1), GetBlockStateId("minecraft:orange_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 2), GetBlockStateId("minecraft:magenta_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 3), GetBlockStateId("minecraft:light_blue_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 4), GetBlockStateId("minecraft:yellow_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 5), GetBlockStateId("minecraft:lime_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 6), GetBlockStateId("minecraft:pink_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 7), GetBlockStateId("minecraft:gray_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 8), GetBlockStateId("minecraft:light_gray_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 9), GetBlockStateId("minecraft:cyan_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 10), GetBlockStateId("minecraft:purple_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 11), GetBlockStateId("minecraft:blue_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 12), GetBlockStateId("minecraft:brown_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 13), GetBlockStateId("minecraft:green_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 14), GetBlockStateId("minecraft:red_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(159, 15), GetBlockStateId("minecraft:black_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 0), GetBlockStateId("minecraft:white_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 1), GetBlockStateId("minecraft:orange_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 2), GetBlockStateId("minecraft:magenta_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 3), GetBlockStateId("minecraft:light_blue_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 4), GetBlockStateId("minecraft:yellow_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 5), GetBlockStateId("minecraft:lime_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 6), GetBlockStateId("minecraft:pink_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 7), GetBlockStateId("minecraft:gray_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 8), GetBlockStateId("minecraft:light_gray_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 9), GetBlockStateId("minecraft:cyan_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 10), GetBlockStateId("minecraft:purple_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 11), GetBlockStateId("minecraft:blue_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 12), GetBlockStateId("minecraft:brown_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 13), GetBlockStateId("minecraft:green_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 14), GetBlockStateId("minecraft:red_stained_glass_pane"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(160, 15), GetBlockStateId("minecraft:black_stained_glass_pane"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 2) _1p12to1p13.Add(BlockFactory.GetBlockStateID(161, Índice_Data), GetBlockStateId("minecraft:acacia_leaves"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 2) _1p12to1p13.Add(BlockFactory.GetBlockStateID(161, Índice_Data), GetBlockStateId("minecraft:dark_oak_leaves"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 2) _1p12to1p13.Add(BlockFactory.GetBlockStateID(162, Índice_Data), GetBlockStateId("minecraft:acacia_log"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 2) _1p12to1p13.Add(BlockFactory.GetBlockStateID(162, Índice_Data), GetBlockStateId("minecraft:dark_oak_log"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(163, 0), GetBlockStateId("minecraft:acacia_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(164, 0), GetBlockStateId("minecraft:dark_oak_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(165, 0), GetBlockStateId("minecraft:slime_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(166, 0), GetBlockStateId("minecraft:barrier"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(167, 0), GetBlockStateId("minecraft:iron_trapdoor"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(168, 0), GetBlockStateId("minecraft:prismarine"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(168, 1), GetBlockStateId("minecraft:dark_prismarine"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(168, 2), GetBlockStateId("minecraft:prismarine_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(169, 0), GetBlockStateId("minecraft:sea_lantern"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(170, 0), GetBlockStateId("minecraft:hay_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 0), GetBlockStateId("minecraft:white_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 1), GetBlockStateId("minecraft:orange_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 2), GetBlockStateId("minecraft:magenta_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 3), GetBlockStateId("minecraft:light_blue_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 4), GetBlockStateId("minecraft:yellow_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 5), GetBlockStateId("minecraft:lime_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 6), GetBlockStateId("minecraft:pink_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 7), GetBlockStateId("minecraft:gray_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 8), GetBlockStateId("minecraft:light_gray_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 9), GetBlockStateId("minecraft:cyan_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 10), GetBlockStateId("minecraft:purple_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 11), GetBlockStateId("minecraft:blue_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 12), GetBlockStateId("minecraft:brown_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 13), GetBlockStateId("minecraft:green_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 14), GetBlockStateId("minecraft:red_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(171, 15), GetBlockStateId("minecraft:black_carpet"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(172, 0), GetBlockStateId("minecraft:terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(173, 0), GetBlockStateId("minecraft:coal_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(174, 0), GetBlockStateId("minecraft:packed_ice"));
			for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:sunflower"));
			for (byte Índice_Data = 1; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:lilac"));
			for (byte Índice_Data = 2; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:tall_grass"));
			for (byte Índice_Data = 3; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:large_fern"));
			for (byte Índice_Data = 4; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:rose_bush"));
			for (byte Índice_Data = 5; Índice_Data < 16; Índice_Data += 8) _1p12to1p13.Add(BlockFactory.GetBlockStateID(175, Índice_Data), GetBlockStateId("minecraft:peony"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 0), GetBlockStateId("minecraft:white_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 1), GetBlockStateId("minecraft:orange_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 2), GetBlockStateId("minecraft:magenta_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 3), GetBlockStateId("minecraft:light_blue_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 4), GetBlockStateId("minecraft:yellow_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 5), GetBlockStateId("minecraft:lime_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 6), GetBlockStateId("minecraft:pink_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 7), GetBlockStateId("minecraft:gray_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 8), GetBlockStateId("minecraft:light_gray_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 9), GetBlockStateId("minecraft:cyan_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 10), GetBlockStateId("minecraft:purple_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 11), GetBlockStateId("minecraft:blue_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 12), GetBlockStateId("minecraft:brown_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 13), GetBlockStateId("minecraft:green_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 14), GetBlockStateId("minecraft:red_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(176, 15), GetBlockStateId("minecraft:black_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 0), GetBlockStateId("minecraft:white_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 1), GetBlockStateId("minecraft:orange_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 2), GetBlockStateId("minecraft:magenta_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 3), GetBlockStateId("minecraft:light_blue_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 4), GetBlockStateId("minecraft:yellow_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 5), GetBlockStateId("minecraft:lime_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 6), GetBlockStateId("minecraft:pink_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 7), GetBlockStateId("minecraft:gray_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 8), GetBlockStateId("minecraft:light_gray_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 9), GetBlockStateId("minecraft:cyan_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 10), GetBlockStateId("minecraft:purple_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 11), GetBlockStateId("minecraft:blue_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 12), GetBlockStateId("minecraft:brown_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 13), GetBlockStateId("minecraft:green_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 14), GetBlockStateId("minecraft:red_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(177, 15), GetBlockStateId("minecraft:black_banner"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(178, 0), GetBlockStateId("minecraft:daylight_detector"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(179, 0), GetBlockStateId("minecraft:red_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(179, 1), GetBlockStateId("minecraft:chiseled_red_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(179, 2), GetBlockStateId("minecraft:cut_red_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(180, 0), GetBlockStateId("minecraft:red_sandstone_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(181, 0), GetBlockStateId("minecraft:red_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(181, 8), GetBlockStateId("minecraft:smooth_red_sandstone"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(182, 0), GetBlockStateId("minecraft:red_sandstone_slab"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(183, 0), GetBlockStateId("minecraft:spruce_fence_gate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(184, 0), GetBlockStateId("minecraft:birch_fence_gate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(185, 0), GetBlockStateId("minecraft:jungle_fence_gate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(186, 0), GetBlockStateId("minecraft:dark_oak_fence_gate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(187, 0), GetBlockStateId("minecraft:acacia_fence_gate"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(188, 0), GetBlockStateId("minecraft:spruce_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(189, 0), GetBlockStateId("minecraft:birch_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(190, 0), GetBlockStateId("minecraft:jungle_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(191, 0), GetBlockStateId("minecraft:acacia_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(192, 0), GetBlockStateId("minecraft:dark_oak_fence"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(193, 0), GetBlockStateId("minecraft:spruce_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(194, 0), GetBlockStateId("minecraft:birch_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(195, 0), GetBlockStateId("minecraft:jungle_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(196, 0), GetBlockStateId("minecraft:acacia_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(197, 0), GetBlockStateId("minecraft:dark_oak_door"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(198, 0), GetBlockStateId("minecraft:end_rod"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(199, 0), GetBlockStateId("minecraft:chorus_plant"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(200, 0), GetBlockStateId("minecraft:chorus_flower"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(201, 0), GetBlockStateId("minecraft:purpur_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(202, 0), GetBlockStateId("minecraft:purpur_pillar"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(203, 0), GetBlockStateId("minecraft:purpur_stairs"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(204, 0), GetBlockStateId("minecraft:purpur_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(205, 0), GetBlockStateId("minecraft:purpur_slab"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(206, 0), GetBlockStateId("minecraft:end_stone_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(207, 0), GetBlockStateId("minecraft:beetroots"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(208, 0), GetBlockStateId("minecraft:grass_path"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(209, 0), GetBlockStateId("minecraft:end_gateway"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(210, 0), GetBlockStateId("minecraft:repeating_command_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(211, 0), GetBlockStateId("minecraft:chain_command_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(212, 0), GetBlockStateId("minecraft:frosted_ice"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(213, 0), GetBlockStateId("minecraft:magma_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(214, 0), GetBlockStateId("minecraft:nether_wart_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(215, 0), GetBlockStateId("minecraft:red_nether_bricks"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(216, 0), GetBlockStateId("minecraft:bone_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(217, 0), GetBlockStateId("minecraft:structure_void"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(218, 0), GetBlockStateId("minecraft:observer"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(219, 0), GetBlockStateId("minecraft:white_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(220, 0), GetBlockStateId("minecraft:orange_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(221, 0), GetBlockStateId("minecraft:magenta_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(222, 0), GetBlockStateId("minecraft:light_blue_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(223, 0), GetBlockStateId("minecraft:yellow_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(224, 0), GetBlockStateId("minecraft:lime_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(225, 0), GetBlockStateId("minecraft:pink_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(226, 0), GetBlockStateId("minecraft:gray_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(227, 0), GetBlockStateId("minecraft:light_gray_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(228, 0), GetBlockStateId("minecraft:cyan_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(229, 0), GetBlockStateId("minecraft:purple_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(230, 0), GetBlockStateId("minecraft:blue_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(231, 0), GetBlockStateId("minecraft:brown_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(232, 0), GetBlockStateId("minecraft:green_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(233, 0), GetBlockStateId("minecraft:red_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(234, 0), GetBlockStateId("minecraft:black_shulker_box"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(235, 0), GetBlockStateId("minecraft:white_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(236, 0), GetBlockStateId("minecraft:orange_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(237, 0), GetBlockStateId("minecraft:magenta_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(238, 0), GetBlockStateId("minecraft:light_blue_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(239, 0), GetBlockStateId("minecraft:yellow_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(240, 0), GetBlockStateId("minecraft:lime_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(241, 0), GetBlockStateId("minecraft:pink_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(242, 0), GetBlockStateId("minecraft:gray_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(243, 0), GetBlockStateId("minecraft:light_gray_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(244, 0), GetBlockStateId("minecraft:cyan_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(245, 0), GetBlockStateId("minecraft:purple_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(246, 0), GetBlockStateId("minecraft:blue_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(247, 0), GetBlockStateId("minecraft:brown_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(248, 0), GetBlockStateId("minecraft:green_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(249, 0), GetBlockStateId("minecraft:red_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(250, 0), GetBlockStateId("minecraft:black_glazed_terracotta"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 0), GetBlockStateId("minecraft:white_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 1), GetBlockStateId("minecraft:orange_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 2), GetBlockStateId("minecraft:magenta_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 3), GetBlockStateId("minecraft:light_blue_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 4), GetBlockStateId("minecraft:yellow_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 5), GetBlockStateId("minecraft:lime_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 6), GetBlockStateId("minecraft:pink_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 7), GetBlockStateId("minecraft:gray_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 8), GetBlockStateId("minecraft:light_gray_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 9), GetBlockStateId("minecraft:cyan_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 10), GetBlockStateId("minecraft:purple_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 11), GetBlockStateId("minecraft:blue_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 12), GetBlockStateId("minecraft:brown_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 13), GetBlockStateId("minecraft:green_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 14), GetBlockStateId("minecraft:red_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(251, 15), GetBlockStateId("minecraft:black_concrete"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 0), GetBlockStateId("minecraft:white_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 1), GetBlockStateId("minecraft:orange_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 2), GetBlockStateId("minecraft:magenta_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 3), GetBlockStateId("minecraft:light_blue_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 4), GetBlockStateId("minecraft:yellow_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 5), GetBlockStateId("minecraft:lime_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 6), GetBlockStateId("minecraft:pink_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 7), GetBlockStateId("minecraft:gray_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 8), GetBlockStateId("minecraft:light_gray_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 9), GetBlockStateId("minecraft:cyan_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 10), GetBlockStateId("minecraft:purple_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 11), GetBlockStateId("minecraft:blue_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 12), GetBlockStateId("minecraft:brown_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 13), GetBlockStateId("minecraft:green_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 14), GetBlockStateId("minecraft:red_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(252, 15), GetBlockStateId("minecraft:black_concrete_powder"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(255, 0), GetBlockStateId("minecraft:structure_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(255, 1), GetBlockStateId("minecraft:structure_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(255, 2), GetBlockStateId("minecraft:structure_block"));
			_1p12to1p13.Add(BlockFactory.GetBlockStateID(255, 3), GetBlockStateId("minecraft:structure_block"));

			// 36, 253, 254
			// Agregar el resto de Datas posibles para cada ID con el valor del primero:
			for (short Índice_ID = 0; Índice_ID < 256; Índice_ID++)
			{
				uint Índice_ID_Data_Cero = BlockFactory.GetBlockStateID(Índice_ID, 0);
				if (!_1p12to1p13.ContainsKey(Índice_ID_Data_Cero))
				{
					_1p12to1p13.Add(Índice_ID_Data_Cero, GetBlockStateId("minecraft:air"));
				}
				for (byte Índice_Data = 0; Índice_Data < 16; Índice_Data++)
				{
					uint Índice_ID_Data = BlockFactory.GetBlockStateID(Índice_ID, Índice_Data);
					if (!_1p12to1p13.ContainsKey(Índice_ID_Data))
					{
						_1p12to1p13.Add(Índice_ID_Data, _1p12to1p13[Índice_ID_Data_Cero]);
						//_1p12to1p13.Add(Índice_ID_Data, _1p12to1p13[BlockFactory.GetBlockStateID(152, 0)));
					}
				}
				//else MessageBox.Show(Índice_ID.ToString(), "No dic");
			}
		}
	}
}
