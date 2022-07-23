using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using fNbt;
using NLog;

namespace Alex.Worlds.Chunks
{
	public class ChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ChunkColumn));

		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X { get; set; }
		public int Z { get; set; }

		public bool IsNew { get; set; } = true;
		public bool CalculateLighting { get; set; } = true;

		protected ChunkSection[] Sections { get; set; }
		protected internal BiomeStorage[] BiomeStorages { get; set; }
		
		//	private readonly int[] _biomeId;
		private readonly short[] _height = new short[256];

		//public object UpdateLock { get; set; } = new object();
		public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
		private readonly SemaphoreSlim DataSemaphore = new SemaphoreSlim(1);

		public object QueueLock = new object();
		public ConcurrentDictionary<BlockCoordinates, NbtCompound> BlockEntities { get; }
		//public  NbtCompound[]                                       GetBlockEntities => BlockEntities.ToArray();

		internal ChunkData ChunkData
		{
			get => _chunkData;
			private set
			{
				var oldValue = _chunkData;
				_chunkData = value;

				if (oldValue != value)
				{
					oldValue?.Dispose();
				}
			}
		}

		public WorldSettings WorldSettings { get; }
		protected readonly int _sectionOffset;
		private ChunkData _chunkData;

		public ChunkColumn(int x, int z, WorldSettings worldSettings)
		{
			X = x;
			Z = z;
			WorldSettings = worldSettings;
			_sectionOffset = worldSettings.MinY < 0 ? Math.Abs(worldSettings.MinY >> 4) : 0;

			int realHeight = worldSettings.WorldHeight + Math.Abs(worldSettings.MinY);

			Sections = new ChunkSection[realHeight / 16];
			BiomeStorages = new BiomeStorage[25];
			
			for (int i = 0; i < Sections.Length; i++)
			{
				Sections[i] = null;
			}

			for (int i = 0; i < BiomeStorages.Length; i++)
			{
				BiomeStorages[i] = null;
			}

			BlockEntities = new ConcurrentDictionary<BlockCoordinates, NbtCompound>();
			//_biomeId = new int[16 * 16 * realHeight];

			ChunkData = ChunkData.Create(x, z);
		}

		public ChunkColumn(int x, int z) : this(x, z, WorldSettings.Default) { }

		public int ChunkSections => Sections.Length;
		public ChunkSection this[int index]
		{
			get
			{
				return Sections[index];
			}
			set
			{
				var oldSection = Sections[index];
				value?.Initialize();
				Sections[index] = value;
				
				if (oldSection != value)
					oldSection?.Dispose();
			}
		}

		private bool CheckWithinCoordinates(int x, int y, int z, bool throwException = true)
		{
			if (y < WorldSettings.MinY || y >= WorldSettings.WorldHeight)
			{
				if (throwException)
					throw new Exception(
						$"Y level is out side of support range. (Min: {WorldSettings.MinY} Max: {WorldSettings.WorldHeight} Value: {y})");

				return false;
			}

			return true;
		}

		protected void SetScheduled(int x, int y, int z, bool value)
		{
			if (!CheckWithinCoordinates(x, y, z, false))
				return;
		}

		public static float AverageUpdateTime => MovingAverage.Average;
		public static float MaxUpdateTime => MovingAverage.Maximum;
		public static float MinUpdateTime => MovingAverage.Minimum;

		private static readonly MovingAverage MovingAverage = new MovingAverage();

		public bool UpdateBuffer(IBlockAccess world, bool applyChanges)
		{
			if (!DataSemaphore.Wait(0))
				return false;

			Stopwatch time = Stopwatch.StartNew();

			try
			{
				var chunkData = ChunkData;

				if (chunkData == null)
					return false;

				world = new OffsetBlockAccess(new BlockCoordinates(X << 4, 0, Z << 4), world);

				for (int sectionIndex = 0; sectionIndex < Sections.Length; sectionIndex++)
				{
					var section = Sections[sectionIndex];

					if (section == null)
						continue;

					var si = sectionIndex;
					si -= _sectionOffset;

					var yOffset = (si << 4);

					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int y = 0; y < 16; y++)
							{
								var blockPosition = new BlockCoordinates(x, yOffset + y, z);

								for (int storage = 0; storage < section.StorageCount; storage++)
								{
									var blockState = section.Get(x, y, z, storage);

									if (blockState == null || blockState?.VariantMapper?.Model == null
									                       || blockState.Block == null || !blockState.Block.Renderable)
										continue;

									var model = blockState.VariantMapper.Model;

									if ((blockState.Block.RequiresUpdate || blockState.VariantMapper.IsMultiPart))
									{
										var newblockState = blockState.Block.BlockPlaced(
											world, blockState, blockPosition);

										if (blockState.Id != newblockState.Id)
										{
											blockState = newblockState;

											section.Set(storage, x, y, z, blockState);
											model = blockState?.VariantMapper?.Model;
										}
									}

									if (model != null)
									{
										model.GetVertices(world, chunkData, blockPosition, blockState);
									}
								}
							}
						}
					}
				}

				if (!_disposed && !chunkData.Disposed)
				{
					chunkData.ApplyChanges(world);
				}
				else
				{
					chunkData?.Dispose();

					return false;
				}

				IsNew = false;
			}
			finally
			{
				DataSemaphore.Release();
				time.Stop();

				MovingAverage.ComputeAverage((float)time.Elapsed.TotalMilliseconds);
			}

			return true;
		}

		public IEnumerable<BlockCoordinates> GetLightSources()
		{
			for (int i = 0; i < Sections.Length; i++)
			{
				var section = Sections[i];

				if (section == null)
					continue;

				var si = i;
				si -= _sectionOffset;

				var yOffset = (si * 16);

				foreach (var ls in section.LightSources.ToArray())
				{
					yield return new BlockCoordinates(ls.X, yOffset + ls.Y, ls.Z);
				}
			}
		}

		protected virtual ChunkSection CreateSection(bool storeSkylight, int sections)
		{
			return new ChunkSection(sections);
		}

		public ChunkSection GetSection(int y)
		{
			if (_disposed)
				return null;

			y = y >> 4;
			y += _sectionOffset;

			if (y >= Sections.Length || y < 0)
			{
				throw new IndexOutOfRangeException(
					$"Y value out of range! Expected a number between 0 & {Sections.Length}, Got: {y}");
			}

			var section = Sections[y];

			if (section == null)
			{
				var storage = CreateSection(true, 2);
				this[y] = storage;

				return storage;
			}

			return (ChunkSection)section;
		}
		
		protected virtual BiomeStorage CreateBiomeStorage()
		{
			return new BiomeStorage();
		}
		
		public BiomeStorage GetBiomes(int y)
		{
			if (_disposed)
				return null;

			y = y >> 4;
			y += _sectionOffset;

			if (y >= BiomeStorages.Length || y < 0)
			{
				throw new IndexOutOfRangeException(
					$"Y value out of range! Expected a number between 0 & {BiomeStorages.Length}, Got: {y}");
			}

			var section = BiomeStorages[y];

			if (section == null)
			{
				var storage = CreateBiomeStorage();
				BiomeStorages[y] = storage;

				return storage;
			}

			return section;
		}

		public void SetBlockState(int x, int y, int z, BlockState blockState)
		{
			SetBlockState(x, y, z, blockState, 0);
		}

		public void SetBlockState(int x, int y, int z, BlockState state, int storage)
		{
			if (!CheckWithinCoordinates(x, y, z, false))
				return;

			if ((x < 0 || x > ChunkWidth) || (z < 0 || z > ChunkDepth))
				return;

			var section = GetSection(y);
			section?.Set(storage, x, y & 0xf, z, state);
		}

		private void RecalculateHeight(int x, int z, bool doLighting = true)
		{
			bool inLight = doLighting;

			bool calculatingHeight = true;

			for (int y = WorldSettings.WorldHeight - 1; y > WorldSettings.MinY; y--)
			{
				var section = GetSection(y);

				if (section == null) continue;

				var block = section.Get(x, y & 0xf, z).Block;

				if (calculatingHeight)
				{
					if (block.Renderable)
					{
						calculatingHeight = false;
						SetHeight(x, z, (short)(y + 1));
					}
				}

				if (inLight)
				{
					if (!block.Renderable || (!block.BlockMaterial.BlocksLight))
					{
						SetSkyLight(x, y, z, 15);
					}
					else
					{
						SetSkyLight(x, y, z, 0);
						inLight = false;
					}
				}

				if (!inLight && !calculatingHeight)
					break;
			}
		}

		public int GetRecalculatedHeight(int x, int z)
		{
			bool isInAir = true;

			for (int y = WorldSettings.WorldHeight - 1; y >= WorldSettings.MinY; y--)
			{
				{
					var chunk = GetSection(y);

					if (chunk == null || (isInAir && chunk.IsAllAir))
					{
						y -= 15;

						continue;
					}

					isInAir = false;

					var block = GetBlockState(x, y, z).Block;

					if (!block.Renderable || (block.Transparent && !block.BlockMaterial.BlocksLight))
						continue;

					return y + 1;
				}
			}

			return 0;
		}

		public void CalculateHeight(bool doLighting = true)
		{
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					RecalculateHeight(x, z, doLighting);
				}
			}

			foreach (var section in Sections)
			{
				section?.RemoveInvalidBlocks();
			}
		}

		private static BlockState Air = BlockFactory.GetBlockState("minecraft:air");

		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int bx, int by, int bz)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
			{
				yield break;
			}

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
			{
				yield break;
			}

			var chunk = GetSection(by);

			if (chunk == null)
			{
				yield break;
			}

			foreach (var bs in chunk.GetAll(bx, by & 0xf, bz))
			{
				yield return bs;
			}
		}

		public BlockState GetBlockState(int bx, int by, int bz)
		{
			return GetBlockState(bx, by, bz, 0);
		}

		public BlockState GetBlockState(int bx, int by, int bz, int storage)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return Air;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return Air;

			var chunk = GetSection(by);

			if (chunk == null) return Air;

			return chunk.Get(bx, by & 0xf, bz, storage) ?? Air;
		}

		public void SetHeight(int bx, int bz, short h)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			_height[((bz << 4) + (bx))] = h;
		}

		public int GetHeight(int bx, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return WorldSettings.WorldHeight;

			return _height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int by, int bz, Biome biome)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = GetBiomes(by);

			if (section == null) return;

			section.Set(bx, by & 0xf, bz, biome);
		}

		public Biome GetBiome(int bx, int by, int bz)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return BiomeUtils.Biomes[0];

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return BiomeUtils.Biomes[0];

			var section = GetBiomes(by);

			if (section == null)
				return BiomeUtils.Biomes[0];

			return section.Get(bx, by & 0xf, bz);
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return 0;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0;

			var section = GetSection(by);

			if (section == null) return 0;

			return (byte)section.GetBlocklight(bx, by & 0xf, bz);
		}

		public bool SetBlocklight(int bx, int by, int bz, byte data)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return false;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return false;

			return GetSection(by)?.SetBlocklight(bx, by & 0xf, bz, data) ?? false;
		}

		public void GetLight(int bx, int by, int bz, out byte skyLight, out byte blockLight)
		{
			skyLight = 0xff;
			blockLight = 0;

			if (!CheckWithinCoordinates(bx, by, bz, false))
				return;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = GetSection(by);

			if (section == null) return;

			section.GetLight(bx, by & 0xf, bz, out skyLight, out blockLight);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return 0xff;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0xff;

			var section = GetSection(by);

			if (section == null) return 0xff;

			return section.GetSkylight(bx, by & 0xf, bz);
		}

		public bool SetSkyLight(int bx, int by, int bz, byte data)
		{
			if (!CheckWithinCoordinates(bx, by, bz, false))
				return false;

			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return false;

			return GetSection(by)?.SetSkylight(bx, by & 0xf, bz, data) ?? false;
		}

		protected int GetCoordinateIndex(int x, int y, int z)
		{
			y += Math.Abs(this.WorldSettings.MinY);

			return (y << 8 | z << 4 | x);
		}

		public void ScheduleBlockUpdate(int x, int y, int z)
		{
			SetScheduled(x, y, z, true);
		}

		public bool AddBlockEntity(BlockCoordinates coordinates, NbtCompound entity)
		{
			return BlockEntities.TryAdd(coordinates, entity);
		}

		public bool TryGetBlockEntity(BlockCoordinates coordinates, out NbtCompound entity)
		{
			return BlockEntities.TryGetValue(coordinates, out entity);
		}

		public bool RemoveBlockEntity(BlockCoordinates coordinates)
		{
			return BlockEntities.TryRemove(coordinates, out _);
		}

		private bool _disposed = false;

		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			var sections = Sections.ToArray();

			for (var index = 0; index < sections.Length; index++)
			{
				var chunksSection = sections[index];
				Sections[index] = null;

				chunksSection?.Dispose();
			}
			
			var biomes = BiomeStorages.ToArray();

			for (var index = 0; index < biomes.Length; index++)
			{
				var chunksSection = biomes[index];
				BiomeStorages[index] = null;

				chunksSection?.Dispose();
			}
			
			var blockEntities = BlockEntities.ToArray();
			BlockEntities.Clear();

			foreach (var blockEntity in blockEntities) { }

			_chunkData?.Dispose();
			_chunkData = null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ChunkColumn()
		{
			Log.Warn($"Disposing via destructor!");
			Dispose(false);
		}

		public void Reset()
		{
			IsNew = true;
		}
	}
}