using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks.Minecraft;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using Alex.ResourcePackLib.Json;
using fNbt;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Worlds
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

	public class ChunkColumn : IChunkColumn
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(SPWorldProvider));

		public const int ChunkHeight = 256;
		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X { get; set; }
		public int Z { get; set; }

		public bool IsAllAir = false;
		public bool IsNew = true;
		public bool IsLoaded = false;
		public bool NeedSave = false;
		public bool IsDirty { get; set; }
		public bool SkyLightDirty { get; set; }

        public IChunkSection[] Sections { get; set; } = new ChunkSection[16];
		public int[] BiomeId = ArrayOf<int>.Create(256, 1);
		public short[] Height = new short[256];
		
		public object VertexLock { get; set; } = new object();
		public object UpdateLock { get; set; } = new object();
		public ScheduleType Scheduled { get; set; } = ScheduleType.Unscheduled;

		//private bool[] _scheduledUpdates = new bool[ChunkWidth * ChunkDepth * ChunkHeight];
		//private bool[] _scheduledLightingUpdates = new bool[ChunkWidth * ChunkDepth * ChunkHeight];
		//private List<int> _scheduledUpdates = new List<int>();
		//private List<int> _scheduledLightingUpdates = new List<int>();

        public ChunkColumn()
		{
			IsDirty = true;
			SkyLightDirty = true;

			for (int i = 0; i < Sections.Length; i++)
			{
				//var b = new ExtendedBlockStorage(i, true);
				Sections[i] = null;
			}
		}

		private void SetDirty()
		{
			IsDirty = true;
			NeedSave = true;
		}

		public void ScheduleBlockUpdate(int x, int y, int z)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			var section = Sections[y >> 4];
			if (section == null) return;
			section.SetScheduled(x, y - ((y >> 4) << 4), z, true);
		// _scheduledUpdates[y << 8 | z << 4 | x] = true;
		//_scheduledUpdates.Add(y << 8 | z << 4 | x);
		}

		private ChunkSection GetSection(int y)
		{
			var section = Sections[y >> 4];
			if (section == null)
			{
				var storage = new ChunkSection(y, true);
				Sections[y >> 4] = storage;
				return storage;
			}

			return (ChunkSection)section;
		}

		public void SetBlockState(int x, int y, int z, IBlockState blockState)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			GetSection(y).Set(x, y - ((y >> 4) << 4), z, blockState);
			SetDirty();

			RecalculateHeight(x, z);

			_heightDirty = true;

			//var section = Sections[y >> 4];
			//if (section == null) return;
			//section.ScheduledUpdates[(y >> 4) << 8 | z << 4 | x] = true;
           // _scheduledUpdates[y << 8 | z << 4 | x] = true;
        }

		private void RecalculateHeight(int x, int z)
		{
			for (int y = 256 - 1; y > 0; --y)
			{
				if (GetBlock(x, y, z).Renderable)
				{
					SetHeight(x, z, (byte) y);
					break;
				}
			}

			GetHeighest();
		}

		private static IBlockState Air = BlockFactory.GetBlockState("minecraft:air");
		public IBlockState GetBlockState(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return Air;

			var chunk = Sections[by >> 4];
			if (chunk == null) return Air;

			by = by - ((@by >> 4) << 4);
			
			return chunk.Get(bx, by, bz);
		}

		public IBlock GetBlock(int bx, int by, int bz)
		{
			var bs = GetBlockState(bx, by, bz);

			if (bs == null) return new Air();

			return bs.Block;
		}

		public void SetBlock(int bx, int by, int bz, IBlock block)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).Set(bx, by - ((@by >> 4) << 4), bz, block.BlockState);
			SetDirty();

			RecalculateHeight(bx, bz);

            _heightDirty = true;
           // _scheduledUpdates[by << 8 | bz << 4 | bx] = true;
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
				return 0;

			return (byte)Height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int bz, int biome)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return;

			BiomeId[(bz << 4) + (bx)] = biome;
			SetDirty();
		}

		public int GetBiome(int bx, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (bz < 0 || bz > ChunkDepth))
				return 0;

			return BiomeId[(bz << 4) + (bx)];
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 0;

			var section = Sections[by >> 4];
			if (section == null) return 0;

			return (byte) section.GetBlocklight(bx, @by - ((@by >> 4) << 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).SetBlocklight(bx, @by - ((@by >> 4) << 4), bz, data);
			
			//_scheduledLightingUpdates[by << 8 | bz << 4 | bx] = true;
			var section = (ChunkSection)Sections[by >> 4];
			if (section == null) return;
			section.ScheduledSkylightUpdates[by << 8 | bz << 4 | bx] = true;
        }

		public byte GetSkylight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 16;

			var section = Sections[by >> 4];
			if (section == null) return 16;

            return section.GetSkylight(bx, by - ((@by >> 4) << 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).SetSkylight(bx, by - ((@by >> 4) << 4), bz, data);
			SkyLightDirty = true;

            //	_scheduledLightingUpdates[by << 8 | bz << 4 | bx] = true;
           // var section = Sections[by >> 4];
          //  if (section == null) return;
           // section.ScheduledSkylightUpdates[(by - 16 * (by >> 4)) << 8 | bz << 4 | bx] = true;
        }

		private Vector3 Position => new Vector3(X * 16, 0, Z*16);

		public NbtCompound[] Entities { get; internal set; }

        #region New Chunk updates / Vertice building 

		private DynamicVertexBuffer RenewVertexBuffer(GraphicsDevice graphicsDevice, VertexPositionNormalTextureColor[] vertices)
		{
            DynamicVertexBuffer buffer = GpuResourceManager.GetBuffer(this, graphicsDevice,
				VertexPositionNormalTextureColor.VertexDeclaration,
				vertices.Length,
				BufferUsage.WriteOnly);

			if (vertices.Length > 0)
			{
				buffer.SetData(vertices);
			}

			return buffer;
		}
	    
		private IndexBuffer RenewIndexBuffer(GraphicsDevice graphicsDevice, int[] vertices)
		{
			IndexBuffer buffer = GpuResourceManager.GetIndexBuffer(this, graphicsDevice, IndexElementSize.ThirtyTwoBits, vertices.Length, BufferUsage.WriteOnly);

			if (vertices.Length > 0)
			{
				buffer.SetData(vertices);
			}

			return buffer;
		}
		
		#endregion

			public async Task<ChunkMesh> GenerateMeshes(IWorld world)
			{
				
            return null;
			}

			/*private class SectionEntry : IDisposable
			{
				public IndexBuffer SolidIndexBuffer { get; set; }
				public IndexBuffer TransparentIndexBuffer { get; set; }

				public DynamicVertexBuffer SolidBuffer { get; set; }
				//public VertexBuffer TransparentBuffer { get; set; }

				public object _lock = new object();

				public void Dispose()
				{
					lock (_lock)
					{
						SolidIndexBuffer?.Dispose();
						TransparentIndexBuffer?.Dispose();
						SolidBuffer?.Dispose();
						//TransparentBuffer?.Dispose();
					}
				}
			}

			private SectionEntry[] SectionBuffers = ArrayOf<SectionEntry>.Create(16, null); */

			public void DrawOpaque(GraphicsDevice device, BasicEffect effect, out int drawnIndices, out int indexSize)
			{
				indexSize = 0;
				drawnIndices = 0;
				/*for (var index = 0; index < SectionBuffers.Length; index++)
				{
					var section = SectionBuffers[index];

					if (section == null /*|| !Monitor.TryEnter(section._lock)*)
						continue;
					try
					{
						var c = section.SolidIndexBuffer;
						var b = section.SolidBuffer;
						if (c.IndexCount == 0) continue;
						if (b.VertexCount == 0) continue;

						device.SetVertexBuffer(b);
						device.Indices = c;

						foreach (var pass in effect.CurrentTechnique.Passes)
						{
							pass.Apply();
							//device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
						}

						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.IndexCount / 3);
						drawnIndices += (b.VertexCount / 3);
						indexSize += c.IndexCount;
					}
					finally
					{
						//Monitor.Exit(section._lock);
					}
				}*/
			}

			public void DrawTransparent(GraphicsDevice device, AlphaTestEffect effect, out int drawnIndices, out int indexSize)
            {
                drawnIndices = 0;
                indexSize = 0;
                /*indexSize = 0;
                drawnIndices = 0;
                for (var index = 0; index < SectionBuffers.Length; index++)
                {
                    var section = SectionBuffers[index];

                    if (section == null /*|| !Monitor.TryEnter(section._lock)*)
                        continue;
                    try
                    {
                        var c = section.TransparentIndexBuffer;
                        var b = section.SolidBuffer;
                        if (c.IndexCount == 0) continue;
                        if (b.VertexCount == 0) continue;

                        device.SetVertexBuffer(b);
                        device.Indices = c;

                        foreach (var pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();
                            //device.DrawPrimitives(PrimitiveType.TriangleList, 0, b.VertexCount /3);
                        }

                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, c.IndexCount / 3);

                        drawnIndices += (b.VertexCount / 3);
                        indexSize += c.IndexCount;
                    }
                    finally
                    {
                        //Monitor.Exit(section._lock);
                    }
                }*/
            }

			public bool HasDirtySubChunks
			{
				get { return Sections != null && Sections.Any(s => s != null && s.IsDirty); }
			}

			public bool IsWallSectionSolid(int face, int y)
			{
				if (y >= 0 && y < Sections.Length)
				{
					var section = Sections[y];
					if (section == null) return false;

					return ((ChunkSection)section).IsFaceSolid((BlockFace) face);
				}

				return false;
			}
			
			internal IEnumerable<BlockFace> CheckNeighbors(ChunkSection section, int y, IWorld world)
			{
				List<BlockFace> faces = new List<BlockFace>();

				var sectionUp = (ChunkSection)Sections[y + 1];
				if (sectionUp != null && sectionUp.IsFaceSolid(BlockFace.Down))
					faces.Add(BlockFace.Up);

				var sectionDown = (ChunkSection)Sections[y - 1];
				if (sectionDown != null && sectionDown.IsFaceSolid(BlockFace.Up))
					faces.Add(BlockFace.Down);

				var eastChunk = world.GetChunkColumn(X + 1, Z);
				if (eastChunk != null && eastChunk.IsWallSectionSolid(3, y))
				{
					faces.Add(BlockFace.East);
				}

				var westChunk = world.GetChunkColumn(X - 1, Z);
				if (westChunk != null && westChunk.IsWallSectionSolid(2, y))
				{
					faces.Add(BlockFace.West);
				}

				var northChunk = world.GetChunkColumn(X, Z + 1);
				if (northChunk != null && northChunk.IsWallSectionSolid(5, y))
				{
					faces.Add(BlockFace.North);
				}

				var southChunk = world.GetChunkColumn(X, Z - 1);
				if (southChunk != null && southChunk.IsWallSectionSolid(4, y))
				{
					faces.Add(BlockFace.South);
				}

				return faces;
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
						if (_chunkMeshCache != null)
						{
							for (var index = 0; index < _chunkMeshCache.Length; index++)
							{
								//var cached = _chunkMeshCache[index];
								//cached?.Mesh?.Dispose();
								//_chunkMeshCache[index] = null;
							}

							//_chunkMeshCache = null;
						}
					}
					else
					{
						if (_chunkMeshCache == null)
							_chunkMeshCache = new ChunkMeshCache[16];
                    }
				}
			}

			private class ChunkMeshCache
			{
				public ChunkMesh Mesh { get; set; }
				public IDictionary<Vector3, ChunkMesh.EntryPosition> Positions { get; set; }
			}

			private ChunkMeshCache[] _chunkMeshCache = null;

        
			

			private bool _heightDirty = true;
		private int _heighest = -1;
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


            return section.IsTransparent(bx, @by - ((@by >> 4) << 4), bz);
        }

		public bool IsSolid(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return true;

            return section.IsSolid(bx, @by - ((@by >> 4) << 4), bz);
		}

	    public bool IsScheduled(int bx, int @by, int bz)
	    {
	        if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
	            return false;

	        var section = Sections[@by >> 4];
	        if (section == null) return false;

	        return section.IsScheduled(bx, @by - ((@by >> 4) << 4), bz);
        }

	    public void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid)
		{
			transparent = false;
			solid = false;
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = Sections[@by >> 4];
			if (section == null) return;

            section.GetBlockData(bx, @by - ((@by >> 4) << 4), bz, out transparent, out solid);
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
			
		//	foreach (var chunksSection in SectionBuffers)
			{
		//		chunksSection?.Dispose();
			}

			if (_chunkMeshCache != null)
			{
				foreach (var meshCache in _chunkMeshCache)
				{
					meshCache?.Mesh.Dispose();
				}
			}

			//	if (Mesh != null)
			//{
			//	Mesh = null;
			//}
		}

		public void CalculateHeight()
		{
            for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 256 - 1; y > 0; --y)
					{
						if (GetBlock(x, y, z).Renderable)
						{
							SetHeight(x, z, (byte)y);
							break;
						}
					}
				}
			}

			GetHeighest();

            foreach (var section in Sections)
            {
                section?.RemoveInvalidBlocks();
            }
        }

        public void Read(MinecraftStream ms, int availableSections, bool groundUp, bool readSkylight)
		{
			try
			{
				//	Stopwatch s = Stopwatch.StartNew();
				//	Log.Debug($"Reading chunk data...");

				for (int sectionY = 0; sectionY < this.Sections.Length; sectionY++)
				{
					var storage = (ChunkSection)this.Sections[sectionY];
					if ((availableSections & (1 << sectionY)) != 0)
					{
						if (storage == null)
						{
							storage = new ChunkSection(sectionY, readSkylight);
						}

					//	storage.Data.Read(ms);
						/*
						for (int y = 0; y < 16; y++)
						{
							for (int z = 0; z < 16; z++)
							{
								for (int x = 0; x < 16; x += 2)
								{
									// Note: x += 2 above; we read 2 values along x each time
									byte value = (byte)ms.ReadByte();

									storage.SetExtBlocklightValue(x, y, z, (byte)(value & 0xF));
									storage.SetExtBlocklightValue(x + 1, y, z, (byte)((value >> 4) & 0xF));
								}
							}
						}

						//if (currentDimension.HasSkylight())
						if (readSkylight)
						{
							for (int y = 0; y < 16; y++)
							{
								for (int z = 0; z < 16; z++)
								{
									for (int x = 0; x < 16; x += 2)
									{
										// Note: x += 2 above; we read 2 values along x each time
										byte value = (byte)ms.ReadByte();

										storage.SetExtSkylightValue(x, y, z, value & 0xF);
										storage.SetExtSkylightValue(x + 1, y, z, (value >> 4) & 0xF);
									}
								}
							}
						}*/
					}
					else
					{
						if (groundUp && (storage == null || !storage.IsEmpty()))
						{
							if (storage == null)
								storage = new ChunkSection(sectionY, readSkylight);
						}
					}

					storage.IsDirty = true;
					this.Sections[sectionY] = storage;
				}

				if (groundUp)
				{
					for (int x = 0; x < 16; x++)
					{
						for (int z = 0; z < 16; z++)
						{
							var biomeId = ms.ReadInt();
							SetBiome(x, z, biomeId);
						}
					}
				}

				for (int i = 0; i < Sections.Length; i++)
				{
					Sections[i]?.RemoveInvalidBlocks();
				}

				CalculateHeight();
			}
			catch (Exception e)
			{
				Log.Warn($"Received supposedly corrupted chunk:" + e);
			}
		}
	}
}
