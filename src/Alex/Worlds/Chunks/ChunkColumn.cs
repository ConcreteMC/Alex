using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Common.Utils;
using Alex.Common.Utils.Vectors;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Singleplayer;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Worlds.Chunks
{
	//TODO: Implement support support for blocks < 0, possibly implement infinite world height.
	public class ChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public const int ChunkHeight = 256;
		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X { get; set; }
		public int Z { get; set; }

		public ChunkCoordinates Coordinates => new ChunkCoordinates(X, Z);

		public           bool           IsNew           { get; set; } = true;
		public           ChunkSection[] Sections { get; set; } = new ChunkSection[16];
		private readonly int[]          _biomeId = ArrayOf<int>.Create(16 * 16 * 256, 1);
		private readonly  short[]        _height  = new short[256];
		
		public  object                                              UpdateLock       { get; set; } = new object();
		public ConcurrentDictionary<BlockCoordinates, NbtCompound> BlockEntities    { get; }
		//public  NbtCompound[]                                       GetBlockEntities => BlockEntities.ToArray();

		internal ChunkData ChunkData { get; private set; }
		private object _dataLock = new object();

		private System.Collections.BitArray _scheduledUpdates;
		public ChunkColumn(int x, int z)
		{
			X = x;
			Z = z;
			
			for (int i = 0; i < Sections.Length; i++)
			{
				Sections[i] = null;
			}
			
			BlockEntities = new ConcurrentDictionary<BlockCoordinates, NbtCompound>();
			_scheduledUpdates = new System.Collections.BitArray((16 * 256 * 16), false);
			
			ChunkData = new ChunkData(x,z);
		}
		
		protected void SetScheduled(int x, int y, int z, bool value)
		{
			var queue = _scheduledUpdates;

			if (queue != null)
			{
				queue[GetCoordinateIndex(x, y, z)] = value;
			}
		}

		public void ScheduleBorder()
		{
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 256; y++)
					{
						if (x == 0 || x == 15 || z == 0 || z == 15)
						{
							SetScheduled(x,y,z, true);
						}
					}
				}
			}
		}

		public static float AverageUpdateTime => MovingAverage.Average;
		public static float MaxUpdateTime => MovingAverage.Maximum;
		public static float MinUpdateTime => MovingAverage.Minimum;

		private static readonly MovingAverage MovingAverage = new MovingAverage();
		
		private bool _bufferDirty = false;
		public bool UpdateBuffer(GraphicsDevice device, IBlockAccess world, bool applyChanges)
		{
			//Monitor.Enter(_dataLock);
			if (!Monitor.TryEnter(_dataLock, 0))
				return false;

			Stopwatch time = Stopwatch.StartNew();
		
			try
			{
				var chunkData     = ChunkData;

				if (chunkData == null)
					return false;

				var scheduleQueue = _scheduledUpdates;

				if (scheduleQueue == null)
					return false;
				
				bool isNew = IsNew;
				bool didChange = false;
				
				var chunkPosition = new Vector3(X << 4, 0, Z << 4);
				world = new OffsetBlockAccess(chunkPosition, world);
				
				for (int sectionIndex = 0; sectionIndex < 16; sectionIndex++)
				{
					var section = Sections[sectionIndex];

					if (section == null)
						continue;

					var sectionOffset = (sectionIndex * 16);

					//var sectionY = (sectionIndex << 4);

					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int y = 0; y < 16; y++)
							{
								var idx = GetCoordinateIndex(x, sectionOffset + y, z);

								bool scheduled = scheduleQueue[idx]; // IsScheduled(x, y, z)

								if ((!isNew && !scheduled))
									continue;

								if (scheduled)
								{
									_scheduledUpdates[idx] = false;
									didChange = true;
								}

								var blockPosition = new BlockCoordinates(x, y + sectionOffset, z);
								chunkData?.Remove(blockPosition);

								for (int storage = 0; storage < section.StorageCount; storage++)
								{
									var blockState = section.Get(x, y, z, storage);

									if (blockState == null || blockState?.VariantMapper?.Model == null
									                       || blockState.Block == null || !blockState.Block.Renderable)
										continue;

									var model = blockState.VariantMapper.Model;

									if (isNew && (blockState.Block.RequiresUpdate
									              || blockState.VariantMapper.IsMultiPart))
									{
										var newblockState = blockState.Block.BlockPlaced(
											world, blockState, blockPosition);

										if (newblockState != blockState)
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

								//	foreach (var bb in blockState.Block.GetBoundingBoxes(blockPosition))
								//	{
								//		_octree.Add(bb);
								//	}
								}

							}
						}
					}
				}

				if (didChange || isNew)
				{
					_bufferDirty = true;
				}
				if (applyChanges && (didChange || isNew))
				{
					ApplyChanges(world, true, chunkData);
				}
				
				//ChunkData = chunkData;

				IsNew = false;
			}
			finally
			{
				//_previousKeepInMemory = keepInMemory;
				Monitor.Exit(_dataLock);
				time.Stop();
				
				MovingAverage.ComputeAverage((float) time.Elapsed.TotalMilliseconds);
			}

			return true;
		}

		private Stopwatch _lastUpdateWatch = new Stopwatch();
		private void ApplyChanges(IBlockAccess world, bool force, ChunkData chunkData)
		{
			if (!_bufferDirty || (!force && _lastUpdateWatch.IsRunning && _lastUpdateWatch.ElapsedMilliseconds < 50))
				return;
			
			//var chunkData = ChunkData;

			if (chunkData == null || chunkData.Disposed )
				return;
			
			chunkData.ApplyChanges(world, force);
			ChunkData = chunkData;
			
			_bufferDirty = false;
			_lastUpdateWatch.Restart();
		}
		
		public IEnumerable<BlockCoordinates> GetLightSources()
		{
			for (int i = 0; i < Sections.Length; i++)
			{
				var section = Sections[i];
				if (section == null)
					continue;
				
				foreach (var ls in section.LightSources.ToArray())
				{
					yield return new BlockCoordinates(ls.X, (i << 4) + ls.Y, ls.Z);
				}
			}
		}
		
		protected virtual ChunkSection CreateSection(bool storeSkylight, int sections)
		{
			return new ChunkSection(storeSkylight, sections);
		}

		public ChunkSection GetSection(int y)
		{
			y = y >> 4;

			if (y >= Sections.Length || y < 0)
			{
				throw new IndexOutOfRangeException($"Y value out of range! Expected a number between 0 & {Sections.Length - 1}, Got: {y}");
			}
			
			var section = Sections[y];
			
			if (section == null)
			{
				var storage = CreateSection(true, 2);
				Sections[y] = storage;
				return storage;
			}

			return (ChunkSection) section;
		}

		public void SetBlockState(int x, int y, int z, BlockState blockState)
		{
			SetBlockState(x, y, z, blockState, 0);
		}

		public void SetBlockState(int x, int y, int z, BlockState state, int storage)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			var section  = GetSection(y);
			//- 16 * (y >> 4)
			section.Set(storage, x, y & 0xf, z, state);

			_scheduledUpdates[GetCoordinateIndex(x, y, z)] = true;
			//	_heightDirty = true;
		}

		private void RecalculateHeight(int x, int z, bool doLighting = true)
		{
			bool inLight = doLighting;

			int currentLightValue = 15;
			for (int y = 255; y > 0; y--)
			{
				if (inLight)
				{
					var section = GetSection(y);
					var block = section.Get(x, y & 0xf, z).Block;

					/*if (block.Renderable && block.BlockState.Model != null)
					{
						foreach(var box in block.BlockState.Model.GetBoundingBoxes(new Vector3((X << 4) + x, y, (Z << 4) + z)))
						{
							_octree.Add(box);
						}
					}*/
					
					
					if (!block.Renderable || (!block.BlockMaterial.BlocksLight))
					{
						SetSkyLight(x, y, z, 15);
					}
					else
					{
						SetHeight(x, z, (short) (y + 1));
						SetSkyLight(x, y, z, 0);
						inLight = false;
					}
				}
				else
				{
					//SetSkyLight(x, y, z, (byte) (doLighting ? 0 : 15));
				}
			}
		}

		public int GetRecalculatedHeight(int x, int z)
		{
			bool isInAir = true;

			for (int y = 255; y >= 0; y--)
			{
				{
					var chunk = GetSection(y);
					if (isInAir && chunk.IsAllAir)
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

			//GetHeighest();

			foreach (var section in Sections)
			{
				section?.RemoveInvalidBlocks();
			}
		}

		private static BlockState Air = BlockFactory.GetBlockState("minecraft:air");

		public IEnumerable<ChunkSection.BlockEntry> GetBlockStates(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
			{
				yield return new ChunkSection.BlockEntry(Air, 0);
				yield break;
			}

			var chunk = Sections[by >> 4];
			if (chunk == null)
			{
				yield return new ChunkSection.BlockEntry(Air, 0);
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
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return Air;

			var chunk = Sections[by >> 4];
			if (chunk == null) return Air;

			return chunk.Get(bx, by & 0xf, bz, storage) ?? Air;
		}

		public void SetHeight(int bx, int bz, short h)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			_height[((bz << 4) + (bx))] = h;
		}

		public byte GetHeight(int bx, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 255;

			return (byte) _height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int by, int bz, int biome)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			_biomeId[(by << 8 | bz << 4 | bx)] = biome;
		}

		public int GetBiome(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0;

			return _biomeId[(by << 8 | bz << 4 | bx)];
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0;

			var section = Sections[by >> 4];
			if (section == null) return 0;

			return (byte) section.GetBlocklight(bx, by & 0xf, bz);
		}

		public bool SetBlocklight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;
			
			return GetSection(by).SetBlocklight(bx, by & 0xf, bz, data);
		}

		public void GetLight(int bx, int by, int bz, out byte skyLight, out byte blockLight)
		{
			skyLight = 0xff;
			blockLight = 0;
			
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = Sections[by >> 4];
			if (section == null) return;

			section.GetLight(bx, by & 0xf, bz, out skyLight, out blockLight);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0xff;

			var section = Sections[by >> 4];
			if (section == null) return 0xff;

			return section.GetSkylight(bx, by  & 0xf, bz);
		}

		public bool SetSkyLight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			return GetSection(by).SetSkylight(bx, by &  0xf, bz, data);
		}

		protected static int GetCoordinateIndex(int x, int y, int z)
		{
			return (y << 8 | z << 4 | x);
		}
		
		public void ScheduleBlockUpdate(int x, int y, int z)
		{
			SetScheduled(x,y,z, true);
		}
		
		public bool AddBlockEntity(BlockCoordinates coordinates, NbtCompound entity)
		{
			//entity.Block = GetBlockState(coordinates.X & 0x0f, coordinates.Y & 0xf, coordinates.Z & 0x0f).Block;
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

		public void Dispose()
		{
			lock (_dataLock)
			{
				for (var index = 0; index < Sections.Length; index++)
				{
					var chunksSection = Sections[index];
					Sections[index] = null;
					
					chunksSection?.Dispose();
				}

				//Sections = null;
				ChunkData?.Dispose();
				ChunkData = null;
				
				_scheduledUpdates = null;
			}
		}
	}
}
