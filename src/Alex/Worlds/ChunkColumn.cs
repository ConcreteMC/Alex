using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.Utils;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.Minecraft;
using Alex.Blocks.State;
using Alex.Blocks.Storage;
using Alex.Networking.Java.Util;
using Alex.ResourcePackLib.Json;
using Alex.Utils;
using fNbt;
using fNbt.Tags;
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

		public ChunkSection[] Sections = new ChunkSection[16];
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
			section.SetScheduled(x, y - 16 * (y >> 4), z, true);
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

			return section;
		}

		public void SetBlockState(int x, int y, int z, IBlockState blockState)
		{
			if ((x < 0 || x > ChunkWidth) || (y < 0 || y > ChunkHeight) || (z < 0 || z > ChunkDepth))
				return;

			GetSection(y).Set(x, y - 16 * (y >> 4), z, blockState);
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

			by = by - 16 * (by >> 4);
			
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

			GetSection(by).Set(bx, by - 16 * (by >> 4), bz, block.BlockState);
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

			return (byte) section.GetExtBlocklightValue(bx, @by - 16 * (@by >> 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).SetExtBlocklightValue(bx, @by - 16 * (@by >> 4), bz, data);
			
			//_scheduledLightingUpdates[by << 8 | bz << 4 | bx] = true;
			var section = Sections[by >> 4];
			if (section == null) return;
			section.ScheduledSkylightUpdates[by << 8 | bz << 4 | bx] = true;
        }

		public byte GetSkylight(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return 16;

			var section = Sections[by >> 4];
			if (section == null) return 16;

            return section.GetExtSkylightValue(bx, @by - 16 * (@by >> 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			GetSection(by).SetExtSkylightValue(bx, @by - 16 * (@by >> 4), bz, data);
			SkyLightDirty = true;

            //	_scheduledLightingUpdates[by << 8 | bz << 4 | bx] = true;
           // var section = Sections[by >> 4];
          //  if (section == null) return;
           // section.ScheduledSkylightUpdates[(by - 16 * (by >> 4)) << 8 | bz << 4 | bx] = true;
        }

		private Vector3 Position => new Vector3(X * 16, 0, Z*16);

		public NbtCompound[] Entities { get; internal set; }

        #region New Chunk updates / Vertice building 

		private VertexBuffer RenewVertexBuffer(GraphicsDevice graphicsDevice, VertexPositionNormalTextureColor[] vertices)
		{
			VertexBuffer buffer = VertexBufferPool.GetBuffer(graphicsDevice,
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
			IndexBuffer buffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, vertices.Length, BufferUsage.WriteOnly);

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

			private class SectionEntry : IDisposable
			{
				public IndexBuffer SolidIndexBuffer { get; set; }
				public IndexBuffer TransparentIndexBuffer { get; set; }

				public VertexBuffer SolidBuffer { get; set; }
				public VertexBuffer TransparentBuffer { get; set; }

				public object _lock = new object();

				public void Dispose()
				{
					lock (_lock)
					{
						SolidIndexBuffer?.Dispose();
						TransparentIndexBuffer?.Dispose();
						SolidBuffer?.Dispose();
						TransparentBuffer?.Dispose();
					}
				}
			}

			private SectionEntry[] SectionBuffers = ArrayOf<SectionEntry>.Create(16, null); 

			public void DrawOpaque(GraphicsDevice device, BasicEffect effect, out int drawnIndices, out int indexSize)
			{
				indexSize = 0;
				drawnIndices = 0;
				for (var index = 0; index < SectionBuffers.Length; index++)
				{
					var section = SectionBuffers[index];

					if (section == null /*|| !Monitor.TryEnter(section._lock)*/)
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
				}
			}

			public void DrawTransparent(GraphicsDevice device, AlphaTestEffect effect, out int drawnIndices, out int indexSize)
			{
				indexSize = 0;
				drawnIndices = 0;
				for (var index = 0; index < SectionBuffers.Length; index++)
				{
					var section = SectionBuffers[index];

					if (section == null /*|| !Monitor.TryEnter(section._lock)*/)
						continue;
					try
					{
						var c = section.TransparentIndexBuffer;
						var b = section.TransparentBuffer;
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
				}
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

					return section.IsFaceSolid((BlockFace) face);
				}

				return false;
			}
			
			private IEnumerable<BlockFace> CheckNeighbors(ChunkSection section, int y, IWorld world)
			{
				List<BlockFace> faces = new List<BlockFace>();

				var sectionUp = Sections[y + 1];
				if (sectionUp != null && sectionUp.IsFaceSolid(BlockFace.Down))
					faces.Add(BlockFace.Up);

				var sectionDown = Sections[y - 1];
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

			public void UpdateChunk(GraphicsDevice device, IWorld world)
			{
				var scheduled = Scheduled;

				long reUseCounter = 0;
				Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)> blocks =
					new Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)>();

				for (var index = Sections.Length - 1; index >= 0; index--)
				{
					var section = Sections[index];
					if (section == null || section.IsEmpty())
					{
						continue;
					}
					
					if (index > 0 && index < Sections.Length - 1)
					{
						if (!section.HasAirPockets && CheckNeighbors(section, index, world).Count() == 6) //All surrounded by solid.
						{
							// Log.Info($"Found section with solid neigbors, skipping.");
							continue;
						}
					}

					var buffer = SectionBuffers[index];
					if (buffer == null || section.ScheduledUpdates.Any(x => x == true) || section.IsDirty)
					{
						var oldBuffer = buffer;

						ChunkMeshCache oldMeshCache = null;
						var hp = _isHighPriority;
						if (hp)
						{
							oldMeshCache = _chunkMeshCache[index];
						}

						object lockObject = new object();
						bool locked = false;
						if (buffer != null)
						{
							//locked = true;
							lockObject = buffer._lock;
						}

						try
						{
							//lock (lockObject)
							{
								ChunkMesh mesh = GenerateSectionMesh(world, oldMeshCache, ref section, index, ref blocks,
									out var positions, ref reUseCounter);

								var vertices = mesh.Vertices;
								var transparentVertices = mesh.TransparentVertices;
								var indexes = mesh.SolidIndexes;
								var transIndexes = mesh.TransparentIndexes;
								if (mesh.SolidIndexes.Length == 0 && mesh.TransparentIndexes.Length == 0)
								{
									if (!section.ScheduledUpdates.Any(x => x))
										section.IsDirty = false;

									/*if (X == 0 && Z == 1)
									{
										Log.Info($"!!!!!!! Not updating cause no mesh data: (x: {X} z: {Z}) {index} Empty: {section.IsEmpty()} " +
										         $"Vertices: {mesh.Vertices.Length + mesh.TransparentVertices.Length} " +
										         $"Indices: {mesh.SolidIndexes.Length + mesh.TransparentIndexes.Length}");
									}*/

									continue;
								}

								if (buffer == null)
								{
									buffer = new SectionEntry()
									{
										SolidBuffer = VertexBufferPool.GetBuffer(device,
											VertexPositionNormalTextureColor.VertexDeclaration, vertices.Length,
											BufferUsage.WriteOnly),
										SolidIndexBuffer = new IndexBuffer(device,
											IndexElementSize.ThirtyTwoBits,
											indexes.Length, BufferUsage.WriteOnly),
										TransparentBuffer = VertexBufferPool.GetBuffer(device,
											VertexPositionNormalTextureColor.VertexDeclaration,
											transparentVertices.Length,
											BufferUsage.WriteOnly),
										TransparentIndexBuffer = new IndexBuffer(device,
											IndexElementSize.ThirtyTwoBits,
											transIndexes.Length, BufferUsage.WriteOnly)
									};


									if (mesh.SolidIndexes.Length > 0)
									{
										buffer.SolidBuffer.SetData(vertices);
										buffer.SolidIndexBuffer.SetData(indexes);
									}

									if (mesh.TransparentIndexes.Length > 0)
									{
										buffer.TransparentBuffer.SetData(transparentVertices);
										buffer.TransparentIndexBuffer.SetData(transIndexes);
									}

								}
								else
								{
									//lock (buffer._lock)
									{
										if (buffer.SolidBuffer.VertexCount != mesh.Vertices.Length)
										{
											var oldVerticeBuffer = buffer.SolidBuffer;
											buffer.SolidBuffer = RenewVertexBuffer(device, vertices);
											oldVerticeBuffer.Dispose();
										}
										else
										{
										//	buffer.SolidBuffer.SetData(mesh.Vertices);
										}

										if (buffer.TransparentBuffer.VertexCount != mesh.TransparentVertices.Length)
										{
											var oldVerticeBuffer = buffer.TransparentBuffer;
											buffer.TransparentBuffer =
												RenewVertexBuffer(device, transparentVertices);
											oldVerticeBuffer.Dispose();
										}
										else
										{
											//buffer.TransparentBuffer.SetData(mesh.TransparentVertices);
										}

										if (buffer.SolidIndexBuffer.IndexCount != mesh.SolidIndexes.Length)
										{
											var oldIndexBuffer = buffer.SolidIndexBuffer;
											buffer.SolidIndexBuffer = RenewIndexBuffer(device, indexes);
											oldIndexBuffer.Dispose();
										}
										/*else if (mesh.SolidIndexes.Length > 0)
										{
											buffer.SolidIndexBuffer.SetData(mesh.SolidIndexes, 0,
												mesh.SolidIndexes.Length, SetDataOptions.Discard);
										}*/

										if (buffer.TransparentIndexBuffer.IndexCount != mesh.TransparentIndexes.Length)
										{
											var oldIndexBuffer = buffer.TransparentIndexBuffer;
											buffer.TransparentIndexBuffer =
												RenewIndexBuffer(device, transIndexes);
											oldIndexBuffer.Dispose();
										}
										/*else if (mesh.TransparentIndexes.Length > 0)
										{
											buffer.TransparentIndexBuffer.SetData(mesh.TransparentIndexes, 0,
												mesh.TransparentIndexes.Length, SetDataOptions.Discard);
										}*/
									}
								}

								SectionBuffers[index] = buffer;

								if (!section.ScheduledUpdates.Any(x => x))
									section.IsDirty = false;

								//Sections[index] = section;

								if (hp)
								{
									_chunkMeshCache[index] = new ChunkMeshCache()
									{
										Mesh = mesh,
										Positions = positions
									};
								}

								if (oldMeshCache != null)
								{
									oldMeshCache?.Mesh?.Dispose();
								}
							}
						}
						finally
						{
							//if (locked)
							//Monitor.Exit(lockObject);
						}
					}
					else
					{
						
					}

					//Sections[index] = section;
				}
			}

			private ChunkMesh GenerateSectionMesh(IWorld world, ChunkMeshCache cachedMesh,
				ref ChunkSection section, int yIndex,
				ref Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)> blocks,
				out IDictionary<Vector3, ChunkMesh.EntryPosition> outputPositions, ref long reUseCounter)
			{
				var scheduled = Scheduled;
				List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();
				List<VertexPositionNormalTextureColor> transparentVertices =
					new List<VertexPositionNormalTextureColor>();
				var positions = new ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition>();

				List<int> transparentIndexes = new List<int>();
				List<int> solidIndexes = new List<int>();

				//Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)> blocks =
				//     new Dictionary<uint, (VertexPositionNormalTextureColor[] vertices, int[] indexes)>();

				for (var y = 0; y < 16; y++)
				for (var x = 0; x < ChunkWidth; x++)
				for (var z = 0; z < ChunkDepth; z++)
				{
					var vector = new Vector3(x, y, z);

					//if (scheduled.HasFlag(ScheduleType.Scheduled) || scheduled.HasFlag(ScheduleType.Border) || scheduled.HasFlag(ScheduleType.Full) || section.IsDirty)
					{

						bool wasScheduled = section.IsScheduled(x, y, z);
						bool wasLightingScheduled = section.IsLightingScheduled(x, y, z);

						/*if ((scheduled.HasFlag(ScheduleType.Border) &&
					         ((x == 0 && z == 0) || (x == ChunkWidth && z == 0) || (x == 0 && z == ChunkDepth)))
					        || (wasScheduled || wasLightingScheduled || scheduled.HasFlag(ScheduleType.Full)))*/
						//if (true)

						bool found = false;

						if (true || (wasScheduled || wasLightingScheduled || scheduled.HasFlag(ScheduleType.Full)
						     || (scheduled.HasFlag(ScheduleType.Lighting) /*&& cachedMesh == null*/) ||
						     (scheduled.HasFlag(ScheduleType.Border) && (x == 0 || x == 15) && (z == 0 || z == 15))))
						{
							var blockState = section.Get(x, y, z);
							if (blockState == null || !blockState.Block.Renderable) continue;

						/*	if (!blocks.TryGetValue(blockState.ID, out var data))
							{
								data = blockState.Model.GetVertices(world, Vector3.Zero, blockState.Block);
								blocks.Add(blockState.ID, data);
							}
							else
							{
								reUseCounter++;

							}*/
						var blockPosition = new Vector3(x, y + (yIndex * 16), z) + Position;
						var data = blockState.Model.GetVertices(world, blockPosition, blockState.Block);
						

							if (data.vertices == null || data.indexes == null || data.vertices.Length == 0 || data.indexes.Length == 0)
								continue;

							bool transparent = blockState.Block.Transparent;

							

							//var data = CalculateBlockVertices(world, section,
							//	yIndex,
							//	x, y, z, out bool transparent);

							if (data.vertices.Length > 0&& data.indexes.Length > 0)
							{
								int startVerticeIndex = transparent ? transparentVertices.Count : solidVertices.Count;
								foreach (var vert in data.vertices)
								{
									//var vertex = vert;
									//var vertex = new VertexPositionNormalTextureColor(vert.Position + blockPosition, Vector3.Zero, vert.TexCoords, vert.Color);
									//vertex.Position += blockPosition;

									if (transparent)
									{
										transparentVertices.Add(vert);
									}
									else
									{
										solidVertices.Add(vert);
									}
								}

								int startIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
								for (int i = 0; i < data.indexes.Length; i++)
								{
									//	var vert = data.vertices[data.indexes[i]];
									var a = data.indexes[i];

									if (transparent)
									{
										//transparentVertices.Add(vert);
										transparentIndexes.Add(startVerticeIndex + a);

										if (a > transparentVertices.Count)
										{
											Log.Warn($"INDEX > AVAILABLE VERTICES {a} > {transparentVertices.Count}");
										}
									}
									else
									{
										//	solidVertices.Add(vert);
										solidIndexes.Add(startVerticeIndex + a);

										if (a > solidVertices.Count)
										{
											Log.Warn($"INDEX > AVAILABLE VERTICES {a} > {solidVertices.Count}");
										}
									}
								}


								positions.TryAdd(vector,
									new ChunkMesh.EntryPosition(transparent, startIndex, data.indexes.Length));
							}


							if (wasScheduled)
								section.SetScheduled(x, y, z, false);

							if (wasLightingScheduled)
								section.SetLightingScheduled(x, y, z, false);
						}
						else if (cachedMesh != null &&
						         cachedMesh.Positions.TryGetValue(vector, out ChunkMesh.EntryPosition position))
						{
							/*var cachedVertices = position.Transparent
								? cachedMesh.Mesh.TransparentVertices
								: cachedMesh.Mesh.Vertices;

							var cachedIndices = position.Transparent
								? cachedMesh.Mesh.TransparentIndexes
								: cachedMesh.Mesh.SolidIndexes;

							if (cachedIndices == null) continue;

							List<int> done = new List<int>();
							Dictionary<int, int> indiceIndexMap = new Dictionary<int, int>();
							for (var index = 0; index < position.Length; index++)
							{
								var u = cachedIndices[position.Index + index];
								if (!done.Contains(u))
								{
									done.Add(u);
									if (position.Transparent)
									{
										if (!indiceIndexMap.ContainsKey(u))
										{
											indiceIndexMap.Add(u, transparentVertices.Count);
										}

										transparentVertices.Add(
											cachedVertices[u]);
									}
									else
									{
										if (!indiceIndexMap.ContainsKey(u))
										{
											indiceIndexMap.Add(u, solidVertices.Count);
										}

										solidVertices.Add(
											cachedVertices[u]);
									}
								}

							}


							int startIndex = position.Transparent ? transparentIndexes.Count : solidIndexes.Count;
							for (int i = 0; i < position.Length; i++)
							{
								var o = indiceIndexMap[cachedIndices[position.Index + i]];
								if (position.Transparent)
								{
									transparentIndexes.Add(o);
								}
								else
								{
									solidIndexes.Add(o);
								}
							}

							//TODO: Find a way to update just the color of the faces, without having to recalculate vertices
							//We could save what vertices belong to what faces i suppose?
							//We could also use a custom shader to light the blocks...

							positions.TryAdd(vector,
								new ChunkMesh.EntryPosition(position.Transparent, startIndex, position.Length));
							found = true;*/
						}
					}
				}

				outputPositions = positions;
				return new ChunkMesh(solidVertices.ToArray(), transparentVertices.ToArray(), solidIndexes.ToArray(),
					transparentIndexes.ToArray());
			}

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


            return section.IsTransparent(bx, @by - 16 * (@by >> 4), bz);
        }

		public bool IsSolid(int bx, int by, int bz)
		{
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return false;

			var section = Sections[@by >> 4];
			if (section == null) return true;

            return section.IsSolid(bx, @by - 16 * (@by >> 4), bz);
		}

		public void GetBlockData(int bx, int by, int bz, out bool transparent, out bool solid)
		{
			transparent = false;
			solid = false;
			if ((bx < 0 || bx > ChunkWidth) || (by < 0 || by > ChunkHeight) || (bz < 0 || bz > ChunkDepth))
				return;

			var section = Sections[@by >> 4];
			if (section == null) return;

            section.GetBlockData(bx, @by - 16 * (@by >> 4), bz, out transparent, out solid);
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
			
			foreach (var chunksSection in SectionBuffers)
			{
				chunksSection?.Dispose();
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
					var storage = this.Sections[sectionY];
					if ((availableSections & (1 << sectionY)) != 0)
					{
						if (storage == null)
						{
							storage = new ChunkSection(sectionY, readSkylight);
						}

						storage.Data.Read(ms);
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
