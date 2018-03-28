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
			throw new NotImplementedException("TODO: Implement a id:meta to blockstate converter.");

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
	}
}
