using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Alex.API;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.State;
using Alex.Entities.BlockEntities;
using Alex.Graphics.Models.Blocks;
using Alex.Networking.Java.Util;
using Alex.Worlds.Abstraction;
using Alex.Worlds.Singleplayer;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Worlds.Chunks
{
	public static class ArrayOf<T> where T : new()
	{
		public static T[] Create(int size, T initialValue)
		{
			T[] array = (T[])Array.CreateInstance(typeof(T), size);
			for (int i = 0; i < array.Length; i++)
				array[i] = initialValue;
			return array;
		}

		public static T[] Create(int size)
		{
			T[] array = (T[])Array.CreateInstance(typeof(T), size);
			for (int i = 0; i < array.Length; i++)
				array[i] = new T();
			return array;
		}
	}

	public class ChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public const int ChunkHeight = 256;
		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X { get; set; }
		public int Z { get; set; }

		public bool IsNew           { get; set; } = true;
		public bool IsDirty         { get; set; }
		public bool SkyLightDirty   =>  Sections != null && Sections.Sum(x => x.SkyLightUpdates) > 0; 
		public bool BlockLightDirty => Sections != null && Sections.Sum(x => x.BlockLightUpdates) > 0; 
		public readonly Stopwatch LightUpdateWatch = new Stopwatch();
		public ChunkSection[] Sections { get; set; } = new ChunkSection[16];
		public int[] BiomeId = ArrayOf<int>.Create(16 * 16 * 256, 1);
		public short[] Height = new short[256];
		
		public  object                                              UpdateLock { get; set; } = new object();
		public  ScheduleType                                        Scheduled { get; set; } = ScheduleType.Unscheduled;
		public  bool UpdatingLighting { get; set; } = false;
		private ConcurrentDictionary<BlockCoordinates, BlockEntity> BlockEntities { get; }
		public  BlockEntity[]                                       GetBlockEntities => BlockEntities.Values.ToArray();
		
		internal ChunkData ChunkData { get; private set; }
		private object _dataLock = new object();
		public ChunkColumn()
		{
			IsDirty = true;
			//SkyLightDirty = true;
			//BlockLightDirty = true;

			for (int i = 0; i < Sections.Length; i++)
			{
				//var b = new ExtendedBlockStorage(i, true);
				Sections[i] = null;
			}
			
			BlockEntities = new ConcurrentDictionary<BlockCoordinates, BlockEntity>();
			LightUpdateWatch.Start();
			
			ChunkData = new ChunkData();
		}

		public void BuildBuffer(GraphicsDevice device, IBlockAccess world)
		{
			lock (_dataLock)
			{
				var chunkPosition = Position;
				for (int sectionIndex = 0; sectionIndex < 16; sectionIndex++)
				{
					var section = Sections[sectionIndex];

					if (section == null)
						continue;

					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							for (int y = 0; y < 16; y++)
							{
								if (!IsNew && !section.IsScheduled(x, y, z))
									continue;

								try
								{
									var by               = (sectionIndex * 16) + y;
									var blockCoordinates = new BlockCoordinates(x, by, z);

									ChunkData.Remove(blockCoordinates);

									var position = chunkPosition + new Vector3(x, by, z);

									foreach (var state in section.GetAll(x, y, z))
									{
										var blockState = state.State;
										if (blockState == null || blockState.Model == null || blockState.Block == null || !blockState.Block.Renderable)
											continue;
										
										var model = blockState.Model;

										if (blockState != null && blockState.Block.RequiresUpdate)
										{
											var newblockState = blockState.Block.BlockPlaced(
												world, blockState, position);

											if (newblockState != blockState)
											{
												blockState = newblockState;

												section.Set(state.Storage, x, y, z, blockState);
												model = blockState.Model;
											}
										}

										if (blockState.IsMultiPart)
										{
											var newBlockState = MultiPartModels.GetBlockState(
												world, position, blockState, blockState.MultiPartHelper);

											if (newBlockState != blockState)
											{
												blockState = newBlockState;

												section.Set(state.Storage, x, y, z, blockState);
												model = blockState.Model;
											}

											// blockState.Block.Update(world, blockPosition);
										}
										
										var vertices = model.GetVertices(world, position, blockState.Block);

										RenderStage targetState = RenderStage.OpaqueFullCube;

										if (blockState.Block.BlockMaterial.IsLiquid)
										{
											targetState = RenderStage.Liquid;
										}
										else if (blockState.Block.Transparent)
										{
											if (blockState.Block.BlockMaterial.IsOpaque)
											{
												targetState = RenderStage.Transparent;
											}
											else
											{
												targetState = RenderStage.Translucent;
											}
										}
										else if (!blockState.Block.IsFullCube)
										{
											targetState = RenderStage.Opaque;
										}

										//foreach (var vertex in vertices.Vertices)
										//{
										//	ChunkData.AddVertex(blockCoordinates, vertex);
										//}

										foreach (var index in vertices.Indexes)
										{
											var vertex   = vertices.Vertices[index];
											int newIndex = ChunkData.AddVertex(blockCoordinates, vertex);

											ChunkData.AddIndex(blockCoordinates, targetState, newIndex);
										}

										if (vertices.AnimatedIndexes != null)
										{
											foreach (var index in vertices.AnimatedIndexes)
											{
												var vertex   = vertices.Vertices[index];
												int newIndex = ChunkData.AddVertex(blockCoordinates, vertex);

												ChunkData.AddIndex(blockCoordinates, RenderStage.Animated, newIndex);
											}
										}

										//ChunkData.ApplyIntermediate();
									}
								}
								finally
								{
									section.SetScheduled(x, y, z, false);
								}
							}
						}
					}
				}
				
				ChunkData.ApplyChanges(device);
			}

			IsNew = false;
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
					yield return new BlockCoordinates(ls.X, (i * 16) + ls.Y, ls.Z);
				}
			}
		}

		public void SetDirty()
		{
			IsDirty = true;
		}

		public void ScheduleBlockUpdate(int x, int y, int z)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			var section = Sections[y >> 4];
			if (section == null) return;
			section.SetScheduled(x, y - 16 * (y >> 4), z, true);
			// _scheduledUpdates[y << 8 | z << 4 | x] = true;
			//_scheduledUpdates.Add(y << 8 | z << 4 | x);
		}

		protected virtual ChunkSection CreateSection(int y, bool storeSkylight, int sections)
		{
			return new ChunkSection(this, y, storeSkylight, sections);
		}

		public ChunkSection GetSection(int y)
		{
			var section = Sections[y >> 4];
			if (section == null)
			{
				var storage = CreateSection(y >> 4, true, 2);
				Sections[y >> 4] = storage;
				return storage;
			}

			return (ChunkSection) section;
		}

		public void SetBlockState(int x, int y, int z, BlockState blockState)
		{
			SetBlockState(x, y, z, blockState, 0);

			//var section = Sections[y >> 4];
			//if (section == null) return;
			//section.ScheduledUpdates[(y >> 4) << 8 | z << 4 | x] = true;
			// _scheduledUpdates[y << 8 | z << 4 | x] = true;
		}

		public void SetBlockState(int x, int y, int z, BlockState state, int storage)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			GetSection(y).Set(storage, x, y - 16 * (y >> 4), z, state);
			SetDirty();

			//RecalculateHeight(x, z);

			_heightDirty = true;
		}

		public void RecalculateHeight(int x, int z, bool doLighting = true)
		{
			bool inLight = doLighting;
			//bool inAir = true;

			for (int y = 255; y > 0; y--)
			{
				if (inLight)
				{
					//var block = GetBlock(x, y, z);
					var section = GetSection(y);
					var block = section.Get(x, y - ((@y >> 4) << 4), z).Block;

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
					SetSkyLight(x, y, z, (byte) (doLighting ? 0 : 15));
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
						if (chunk.IsDirty) Array.Fill<byte>(chunk.SkyLight.Data, 0xff);
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

		//	by = by - ((@by >> 4) << 4);

			foreach (var bs in chunk.GetAll(bx, by - 16 * (by >> 4), bz))
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

			return chunk.Get(bx, by - 16 * (by >> 4), bz, storage);
		}

		public void SetHeight(int bx, int bz, short h)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			Height[((bz << 4) + (bx))] = h;
			SetDirty();
		}

		public byte GetHeight(int bx, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 255;

			return (byte) Height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int by, int bz, int biome)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			BiomeId[(by << 8 | bz << 4 | bx)] = biome;
			SetDirty();
		}

		public int GetBiome(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0;

			return BiomeId[(by << 8 | bz << 4 | bx)];
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0;

			var section = Sections[by >> 4];
			if (section == null) return 0;

			return (byte) section.GetBlocklight(bx, by - 16 * (by >> 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;
			
			GetSection(by).SetBlocklight(bx, by - 16 * (by >> 4), bz, data);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0xff;

			var section = Sections[by >> 4];
			if (section == null) return 0xff;

			return section.GetSkylight(bx, by - 16 * (by >> 4), bz);
		}

		public bool SetSkyLight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			bool dirty = GetSection(by).SetSkylight(bx, by - 16 * (by >> 4), bz, data);
			return dirty;
		}
		
		public bool HasLightUpdateScheduled(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			return GetSection(by).IsSkylightUpdateScheduled(bx, by - 16 * (by >> 4), bz)
			       || GetSection(by).IsBlockLightScheduled(bx, by - 16 * (by >> 4), bz);
		}
		
		public void SetLightUpdateScheduled(int bx, int by, int bz, bool skyLight, bool blockLight)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).SetSkyLightUpdateScheduled(bx, by - 16 * (by >> 4), bz, skyLight);
			GetSection(by).SetBlockLightScheduled(bx, by - 16 * (by >> 4), bz, blockLight);
		}

		private Vector3 Position => new Vector3(X * 16, 0, Z * 16);

	//	public NbtCompound[] Entities { get; internal set; }

		public bool HasDirtySubChunks
		{
			get { return Sections != null && Sections.Any(s => s != null && s.IsDirty); }
		}

		private bool _isHighPriority = false;

		public bool HighPriority
		{
			get { return _isHighPriority; }
			set
			{
				_isHighPriority = value;
				if (!value)
				{
				}
				else
				{
				
				}
			}
		}

		private bool _heightDirty = true;
		private int _heighest = 256;

		public int GetHeighest()
		{
			if (_heightDirty)
			{
				_heighest = Height.Max();
				_heightDirty = false;
			}

			return _heighest;
		}

		public bool IsTransparent(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return true;


			return section.IsTransparent(bx, @by & 0xf, bz);
		}

		public bool IsSolid(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return true;

			return section.IsSolid(bx, @by & 0xf, bz);
		}

		public bool IsScheduled(int bx, int @by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return false;

			return section.IsScheduled(bx, @by & 0xf, bz);
		}

		public void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid)
		{
			transparent = false;
			solid = false;
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = Sections[@by >> 4];
			if (section == null) return;

			section.GetBlockData(bx, @by & 0xf, bz, out transparent, out solid);
		}
		
		public bool AddBlockEntity(BlockCoordinates coordinates, BlockEntity entity)
		{
			entity.Block = GetBlockState(coordinates.X, coordinates.Y, coordinates.Z).Block;
			//entity.KnownPosition = coordinates;
			//entity.KnownPosition = new PlayerLocation(Position.X + coordinates.X, Position.Y + coordinates.Y, Position.Z + coordinates.Z);
			return BlockEntities.TryAdd(coordinates, entity);
		}

		public bool TryGetBlockEntity(BlockCoordinates coordinates, out BlockEntity entity)
		{
			return BlockEntities.TryGetValue(coordinates, out entity);
		}
	    
		public bool RemoveBlockEntity(BlockCoordinates coordinates)
		{
			return BlockEntities.TryRemove(coordinates, out _);
		}

		public void Dispose()
		{
			/*if (TransparentVertexBuffer != null)
			{
				if (!TransparentVertexBuffer.IsDisposed)
				{
					TransparentVertexBuffer.Dispose();
				}

				TransparentVertexBuffer = null;
			}*/

			foreach (var chunksSection in Sections)
			{
				chunksSection?.Dispose();
			}
			
			ChunkData?.Dispose();
			
			//	if (Mesh != null)
			//{
			//	Mesh = null;
			//}
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

			GetHeighest();

			foreach (var section in Sections)
			{
				section?.RemoveInvalidBlocks();
			}
		}
	}
}
