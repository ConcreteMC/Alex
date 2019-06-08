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

		public ExtendedBlockStorage[] Sections = new ExtendedBlockStorage[16];
		public int[] BiomeId = ArrayOf<int>.Create(256, 1);
		public short[] Height = new short[256];

		public VertexBuffer VertexBuffer { get; set; } = null;

		public IndexBuffer IndexBuffer { get; set; } = null;

		public VertexBuffer TransparentVertexBuffer { get; set; } = null;
		public IndexBuffer TransparentIndexBuffer { get; set; } = null;
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

		private ExtendedBlockStorage GetSection(int y)
		{
			var section = Sections[y >> 4];
			if (section == null)
			{
				var storage = new ExtendedBlockStorage(y, true);
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
        private Dictionary<Vector3, ChunkMesh.EntryPosition> PositionCache = new Dictionary<Vector3, ChunkMesh.EntryPosition>();
		
		private VertexBuffer RenewVertexBuffer(GraphicsDevice graphicsDevice, VertexPositionNormalTextureColor[] vertices)
		{
			VertexBuffer buffer = new VertexBuffer(graphicsDevice,
				VertexPositionNormalTextureColor.VertexDeclaration,
				vertices.Length,
				BufferUsage.WriteOnly);

			if (vertices.Length > 0)
			{
				buffer.SetData(vertices);
			}

			return buffer;
		}
	    
		private DynamicIndexBuffer RenewIndexBuffer(GraphicsDevice graphicsDevice, int[] vertices)
		{
			DynamicIndexBuffer buffer = new DynamicIndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, vertices.Length, BufferUsage.WriteOnly);

			if (vertices.Length > 0)
			{
				buffer.SetData(vertices);
			}

			return buffer;
		}
		/*
		private int[] SolidIndexes { get; set; }
		private int[] TransparentIndexes { get; set; }
		//	private FreePosition FreeSolidIndexes { get; set; }
		private LinkedList<int> FreeIndexes { get; set; } = new LinkedList<int>();
		private LinkedList<int> FreeTransparentIndexes { get; set; } = new LinkedList<int>();
		public async Task UpdateChunk(GraphicsDevice device, IWorld world)
		{
			var scheduled = Scheduled;
			
			var opaqueVertexBuffer = VertexBuffer;
			IndexBuffer opaqueIndexBuffer = null;
			
			var transparentVertexBuffer = TransparentVertexBuffer;
			IndexBuffer transparentIndexBuffer = null;
			
			if (opaqueVertexBuffer == null)
			{
				var mesh = await GenerateMeshes(world);
				
				opaqueVertexBuffer = RenewVertexBuffer(device, mesh.Vertices);
				opaqueIndexBuffer = RenewIndexBuffer(device, mesh.SolidIndexes);
				
				transparentVertexBuffer = RenewVertexBuffer(device, mesh.TransparentVertices);
				transparentIndexBuffer = RenewIndexBuffer(device, mesh.TransparentIndexes);
				
				VertexBuffer oldBuffer;
				IndexBuffer oldIndexBuffer;
				VertexBuffer oldTransparentBuffer;
				IndexBuffer oldTransparentIndexBuffer;
				//lock (VertexLock)
				{
					oldIndexBuffer = IndexBuffer;
					oldBuffer = VertexBuffer;
					VertexBuffer = opaqueVertexBuffer;
					IndexBuffer = opaqueIndexBuffer;
					
					oldTransparentIndexBuffer = TransparentIndexBuffer;
					oldTransparentBuffer = TransparentVertexBuffer;
					TransparentVertexBuffer = transparentVertexBuffer;
					TransparentIndexBuffer = transparentIndexBuffer;

					SolidIndexes = mesh.SolidIndexes;
					TransparentIndexes = mesh.TransparentIndexes;
				}

				oldBuffer?.Dispose();
				oldIndexBuffer?.Dispose();
				
				oldTransparentBuffer?.Dispose();
				oldTransparentIndexBuffer?.Dispose();
			}
			else if (scheduled.HasFlag(ScheduleType.Border) || scheduled.HasFlag(ScheduleType.Scheduled))
			{
				ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition> positions = new ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition>();

				//bool[] originalSchedule = _scheduledUpdates.ToArray();
			//	bool[] originalLightingSchedule = _scheduledLightingUpdates.ToArray();
				
				var previousTransparentIndexes = TransparentIndexes;
				var previousSolidIndexes = SolidIndexes;
				
				List<int> solidIndexes = new List<int>();
				List<int>  transparentIndexes = new List<int>();
				
				var oldPositions = new Dictionary<Vector3, ChunkMesh.EntryPosition>(PositionCache);
				for (var index = 0; index < Sections.Length; index++)
				{
					var section = Sections[index];
					if (section.IsEmpty())
					{
						continue;
					}

					var index1 = index;
					for (var y = 0; y < 16; y++)
					for (var x = 0; x < ChunkWidth; x++)
					for (var z = 0; z < ChunkDepth; z++)
					{
						var scheduleIndex = y << 8 | z << 4 | x;
						bool wasScheduled = section.ScheduledUpdates[scheduleIndex];
						bool wasLightingScheduled = section.ScheduledSkylightUpdates[scheduleIndex];

                                var vector3 = new Vector3(x, (index1 * 16) + y, z);

						if ((scheduled.HasFlag(ScheduleType.Border) &&
						     ((x == 0 && z == 0) || (x == ChunkWidth && z == 0) || (x == 0 && z == ChunkDepth)))
						    || wasScheduled || wasLightingScheduled)
						{
							if (wasScheduled)
								section.ScheduledUpdates[scheduleIndex] = false;

							if (wasLightingScheduled)
							{
                                        //TODO: Only update the face colors
                               section.ScheduledSkylightUpdates[scheduleIndex] = false;
							}

							bool didUpdate = false;
							var newValue = Update(world, section, index1, x, y, z, out bool transparent);
							
							var newIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
							if (oldPositions.TryGetValue(vector3, out var previousPosition))
							{
								if (newValue.indexes.Length > previousPosition.Length) //Our new data is bigger...
								{
									//TODO: Find space for new for data...
								}
								else if (newValue.indexes.Length < previousPosition.Length) //Our new data is smaller
								{
									if (transparent)
									{
										var startIndex = previousTransparentIndexes[previousPosition.Index];
										if (newValue.indexes.Length > 0)
										{									
											didUpdate = true;
											
											//lock (VertexLock)
											{
												TransparentVertexBuffer.SetData(newValue.vertices, startIndex,
													newValue.vertices.Length);
											}
											
											transparentIndexes.AddRange(newValue.indexes.Select(idx => startIndex + idx));
										}
									}
									else
									{
										var startIndex = previousSolidIndexes[previousPosition.Index];
										if (newValue.indexes.Length > 0)
										{
											didUpdate = true;

											//lock (VertexLock)
											{
												VertexBuffer.SetData(newValue.vertices, startIndex,
													newValue.vertices.Length);
											}
											
											solidIndexes.AddRange(newValue.indexes.Select(idx => startIndex + idx));
										}
									}

									for (int i = newValue.indexes.Length; i < previousPosition.Length; i++)
									{
										if (transparent)
										{
											FreeTransparentIndexes.AddLast(previousTransparentIndexes[previousPosition.Index + i]);
										}
										else
										{
											FreeIndexes.AddLast(previousSolidIndexes[previousPosition.Index + i]);
										}
									}
								}
							}
							else //There was no block here before!
							{
								//TODO: Find a place for the block vertices to be stored in the vertexbuffer.
							}

							if (didUpdate)
							{
								positions.TryAdd(vector3,
									new ChunkMesh.EntryPosition(transparent, newIndex,
										newValue.indexes.Length));
							}
						}
						else
						{
							var originalSchedule = section.ScheduledUpdates;

                            bool force = IsUpdateScheduled(vector3 + Vector3.Forward, originalSchedule,
								             null) ||
							             IsUpdateScheduled(vector3 + Vector3.Backward, originalSchedule,
								             null) ||
							             IsUpdateScheduled(vector3 + Vector3.Left, originalSchedule,
								             null) ||
							             IsUpdateScheduled(vector3 + Vector3.Right, originalSchedule,
								             null) ||
							             IsUpdateScheduled(vector3 + Vector3.Up, originalSchedule,
								             null) ||
							             IsUpdateScheduled(vector3 + Vector3.Down, originalSchedule,
								             null);

							if (force)
							{
								var newValue = Update(world, section, index1, x, y, z, out bool transparent);
								var newIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
								
								//TODO ADD!!!

								if (!transparent)
								{
									if (FreeIndexes.Count > 0)
									{
										var first = FreeIndexes.First;
										for (int i = 0; i < newValue.vertices.Length; i++)
										{
											var vertex = newValue.vertices[i];
											VertexBuffer.SetData(0, new VertexPositionNormalTextureColor[]
											{
												vertex
											}, 0, 1, VertexPositionNormalTextureColor.VertexDeclaration.VertexStride);
											first = first.Next;
										}
										solidIndexes.Add(first.Value);
									}
								}
							}
							else if (oldPositions.TryGetValue(vector3, out var oldPosition))
							{
								var originalIndex = oldPosition.Transparent
									? transparentIndexes.Count
									: solidIndexes.Count;

								for (int i = 0; i < oldPosition.Length; i++)
								{
									if (oldPosition.Transparent)
									{
										transparentIndexes.Add(previousTransparentIndexes[oldPosition.Index + i]);
									}
									else
									{
										solidIndexes.Add(previousSolidIndexes[oldPosition.Index + i]);
									}
								}

								positions.TryAdd(vector3,
									new ChunkMesh.EntryPosition(oldPosition.Transparent, originalIndex,
										oldPosition.Length));
							}
						}
					}
				}
				
				if (solidIndexes.Count <= IndexBuffer.IndexCount)
					IndexBuffer.SetData(solidIndexes.ToArray());
				else
				{
					var old = IndexBuffer;
					IndexBuffer = RenewIndexBuffer(device, solidIndexes.ToArray());
					old.Dispose();
				}
				
				if (transparentIndexes.Count <= TransparentIndexBuffer.IndexCount)
					TransparentIndexBuffer.SetData(transparentIndexes.ToArray());
				else
				{
					var old = TransparentIndexBuffer;
					TransparentIndexBuffer = RenewIndexBuffer(device, transparentIndexes.ToArray());
					old.Dispose();
				}
				
				SolidIndexes = solidIndexes.ToArray();
				TransparentIndexes = transparentIndexes.ToArray();
				
				//TransparentIndexes = transparentIndexes.ToArray();
				//SolidIndexes = solidIndexes.ToArray();
				
				PositionCache = new Dictionary<Vector3, ChunkMesh.EntryPosition>(positions);
			}
		}*/
		#endregion

        //public ChunkMesh Mesh { get; set; } = null;

     /*   public async Task<ChunkMesh> GenerateMeshes(IWorld world)
		{
			var scheduled = Scheduled;
			List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();
			List<VertexPositionNormalTextureColor> transparentVertices = new List<VertexPositionNormalTextureColor>();
			ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition> positions = new ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition>();
			
			List<int> transparentIndexes = new List<int>();
			List<int> solidIndexes = new List<int>();

/*
			for (var index = 0; index < Sections.Length; index++)
			{
				var section = Sections[index];
				if (section.IsEmpty())
				{
					continue;
				}

				var index1 = index;

				for (var y = 0; y < 16; y++)
				for (var x = 0; x < ChunkWidth; x++)
				for (var z = 0; z < ChunkDepth; z++)
				{
					var scheduleIndex = (index1 * 16 + y) << 8 | z << 4 | x;
					bool wasScheduled = _scheduledUpdates[scheduleIndex];
					bool wasLightingScheduled = _scheduledUpdates[scheduleIndex];

					if (wasScheduled)
						_scheduledUpdates[scheduleIndex] = false;

					if (wasLightingScheduled)
						_scheduledLightingUpdates[scheduleIndex] = false;

					var data = Update(world, section,
						index1,
						x, y, z, out bool transparent);

					int initialIndex = transparent ? transparentVertices.Count : solidVertices.Count;
					foreach (var vert in data.vertices)
					{
						if (transparent)
						{
							transparentVertices.Add(vert);
						}
						else
						{
							solidVertices.Add(vert);
						}
					}

					int initialIndexIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
					for (int i = 0; i < data.indexes.Length; i++)
					{
						//	var vert = data.vertices[data.indexes[i]];

						if (transparent)
						{
							//transparentVertices.Add(vert);
							transparentIndexes.Add(initialIndex + data.indexes[i]);
						}
						else
						{
							//	solidVertices.Add(vert);
							solidIndexes.Add(initialIndex + data.indexes[i]);
						}
					}

					if (data.vertices.Length > 0)
					{
						positions.TryAdd(new Vector3(x, (index1 * 16) + y, z),
							new ChunkMesh.EntryPosition(transparent, initialIndexIndex, data.indexes.Length));
					}
				}
			}*/

			/*for (int i = solidVertices.Count; i < ChunkWidth * ChunkDepth * ChunkHeight; i++)
			{
				solidVertices.Add(new VertexPositionNormalTextureColor(Vector3.Zero, Vector3.Zero, Vector2.Zero));
				FreeIndexes.AddLast(i);
			}
			
			for (int i = transparentVertices.Count; i < ChunkWidth * ChunkDepth * ChunkHeight; i++)
			{
				transparentVertices.Add(new VertexPositionNormalTextureColor(Vector3.Zero, Vector3.Zero, Vector2.Zero));
				FreeTransparentIndexes.AddLast(i);
			}*


			if ((scheduled.HasFlag(ScheduleType.Border) || scheduled.HasFlag(ScheduleType.Scheduled)
				    /*|| scheduled.HasFlag(ScheduleType.Lighting)*)
			    && VertexBuffer != null && TransparentVertexBuffer != null)
			{
				//bool[] originalSchedule = _scheduledUpdates.ToArray();
				//bool[] originalLightingSchedule = _scheduledLightingUpdates.ToArray();

				//VertexPositionNormalTextureColor[] transparents = Mesh.TransparentVertices;
				//VertexPositionNormalTextureColor[] solids = Mesh.Vertices;
		
			/*	lock (VertexLock)
				{
					var solidCount = VertexBuffer.VertexCount;
					solids =
						new VertexPositionNormalTextureColor[solidCount];
					VertexBuffer.GetData(solids, 0, solidCount);
		
					var count = TransparentVertexBuffer.VertexCount;
					transparents =
						new VertexPositionNormalTextureColor[count];
					TransparentVertexBuffer.GetData(transparents, 0, count);
				}
		*
				//var oldPositions = new Dictionary<Vector3, ChunkMesh.EntryPosition>(PositionCache);
		
				Stopwatch sw = Stopwatch.StartNew();
				for (var index = 0; index < Sections.Length; index++)
				{
					var section = Sections[index];
					if (section == null || section.IsEmpty())
					{
						continue;
					}

					var originalSchedule = section.ScheduledUpdates;
		
					var index1 = index;
					for (var y = 0; y < 16; y++)
						for (var x = 0; x < ChunkWidth; x++)
						for (var z = 0; z < ChunkDepth; z++)
						{
							var scheduleIndex = y << 8 | z << 4 | x;
							bool wasScheduled = section.ScheduledUpdates[scheduleIndex];
							bool wasLightingScheduled = section.ScheduledSkylightUpdates[scheduleIndex];
		
							var vector3 = new Vector3(x, (index1 * 16) + y, z);
		
							if ((scheduled.HasFlag(ScheduleType.Border) &&
								 ((x == 0 && z == 0) || (x == ChunkWidth && z == 0) || (x == 0 && z == ChunkDepth)))
								|| wasScheduled || wasLightingScheduled)
							{
								if (wasScheduled)
									section.ScheduledUpdates[scheduleIndex] = false;
		
								if (wasLightingScheduled)
								{
									//TODO: Only update the face colors
									section.ScheduledSkylightUpdates[scheduleIndex] = false;
								}
		
								var data = Update(world, section,
									index1,
									x, y, z/*,
									solidVertices,
									transparentVertices, out int idx, out int length*, out bool transparent);
								if (data.vertices.Length > 0)
								{
									int initialIndex = transparent ? transparentVertices.Count : solidVertices.Count;
									foreach (var vert in data.vertices)
									{
										if (transparent)
										{
											transparentVertices.Add(vert);
										}
										else
										{
											solidVertices.Add(vert);
										}
									}

									int initialIndexIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
									for (int i = 0; i < data.indexes.Length; i++)
									{
										//	var vert = data.vertices[data.indexes[i]];

										if (transparent)
										{
											//transparentVertices.Add(vert);
											transparentIndexes.Add(initialIndex + data.indexes[i]);
										}
										else
										{
											//	solidVertices.Add(vert);
											solidIndexes.Add(initialIndex + data.indexes[i]);
										}
									}


									positions.TryAdd(vector3,
										new ChunkMesh.EntryPosition(transparent, initialIndexIndex,
											data.indexes.Length));
                                    }
							}
							else
							{
								bool force = IsUpdateScheduled(vector3 + Vector3.Forward, originalSchedule,
										         null) ||
									         IsUpdateScheduled(vector3 + Vector3.Backward, originalSchedule,
										         null) ||
									         IsUpdateScheduled(vector3 + Vector3.Left, originalSchedule,
										         null) ||
									         IsUpdateScheduled(vector3 + Vector3.Right, originalSchedule,
										         null) ||
									         IsUpdateScheduled(vector3 + Vector3.Up, originalSchedule,
										         null) ||
									         IsUpdateScheduled(vector3 + Vector3.Down, originalSchedule,
										         null);
		
								if (true)
								{
									var data = Update(world, section,
										index1,
										x, y, z,/*
										solidVertices,
										transparentVertices, out int idx, out int length, *out bool transparent);
									if (data.vertices.Length > 0)
									{
										int initialIndex =
											transparent ? transparentVertices.Count : solidVertices.Count;
										foreach (var vert in data.vertices)
										{
											if (transparent)
											{
												transparentVertices.Add(vert);
											}
											else
											{
												solidVertices.Add(vert);
										}
										}

										int initialIndexIndex =
											transparent ? transparentIndexes.Count : solidIndexes.Count;
										for (int i = 0; i < data.indexes.Length; i++)
										{
											//	var vert = data.vertices[data.indexes[i]];

											if (transparent)
											{
												//transparentVertices.Add(vert);
												transparentIndexes.Add(initialIndex + data.indexes[i]);
											}
											else
											{
												//	solidVertices.Add(vert);
												solidIndexes.Add(initialIndex + data.indexes[i]);
											}
										}


										positions.TryAdd(vector3,
											new ChunkMesh.EntryPosition(transparent, initialIndexIndex,
												data.indexes.Length));
									}
								}
								/*else if (oldPositions.TryGetValue(vector3, out var position))
								{
									List<VertexPositionNormalTextureColor> vertices = new List<VertexPositionNormalTextureColor>();
									
									for (int theIndex = 0; theIndex < position.Length; theIndex++)
									{
										var idx = position.Transparent ? Mesh.TransparentIndexes[position.Index + theIndex] : Mesh.SolidIndexes[position.Index + theIndex];
										
										vertices.Add(/*position.Transparent
											? transparents[idx]
											: solids[idx]);
									}

									var usedIndex = 0;
									/*if (position.Transparent)
									{
										usedIndex = transparentVertices.Count;
										transparentVertices.AddRange(vertices);
									}
									else
									{
										usedIndex = solidVertices.Count;
										solidVertices.AddRange(vertices);
											//}
									
									/*int usedIndex = 0;
									if (position.Transparent)
									{
										usedIndex = transparentVertices.Count;
										Array.Copy(transparents, position.Index, vertices, 0, vertices.Length);
										transparentVertices.AddRange(vertices);
									}
									else
									{
										usedIndex = solidVertices.Count;
										Array.Copy(solids, position.Index, vertices, 0, vertices.Length);
										solidVertices.AddRange(vertices);
									}
									
									var originalIndex = position.Transparent
										? transparentIndexes.Count
										: solidIndexes.Count;

									for (int i = 0; i < position.Length; i++)
									{
										if (position.Transparent)
										{
											transparentIndexes.Add(usedIndex + i);
										}
										else
										{
											solidIndexes.Add(usedIndex + i);
										}
									}
									
									positions.TryAdd(vector3,
										new ChunkMesh.EntryPosition(position.Transparent, originalIndex,
											position.Length));
									
									//res.Add((vertices, 0, vertices.Length, position.Transparent));
		
									//positions.TryAdd(vector3, new ChunkMesh.EntryPosition(position.Transparent, usedIndex,
									//	position.Length));
								}*
		
								//else
							}
						}
		
				}
				sw.Stop();
				//Log.Info($"Re-build took: {sw.ElapsedMilliseconds}ms - Mode: {scheduled.ToString()}");
			}
			else
			{
				for (var index = 0; index < Sections.Length; index++)
				{
					var section = Sections[index];
					if (section == null || section.IsEmpty())
					{
						continue;
					}

					var index1 = index;

					for (var y = 0; y < 16; y++)
					for (var x = 0; x < ChunkWidth; x++)
					for (var z = 0; z < ChunkDepth; z++)
					{
						var scheduleIndex = y << 8 | z << 4 | x;
						bool wasScheduled = section.ScheduledUpdates[scheduleIndex];
						bool wasLightingScheduled = section.ScheduledSkylightUpdates[scheduleIndex];

						if (wasScheduled)
							section.ScheduledUpdates[scheduleIndex] = false;

						if (wasLightingScheduled)
							section.ScheduledSkylightUpdates[scheduleIndex] = false;

						var data = Update(world, section,
							index1,
							x, y, z, out bool transparent);
						
						if (data.vertices.Length > 0)
						{
							int initialIndex = transparent ? transparentVertices.Count : solidVertices.Count;
							foreach (var vert in data.vertices)
							{
								if (transparent)
								{
									transparentVertices.Add(vert);
								}
								else
								{
									solidVertices.Add(vert);
								}
							}

							int initialIndexIndex = transparent ? transparentIndexes.Count : solidIndexes.Count;
							for (int i = 0; i < data.indexes.Length; i++)
							{
								//	var vert = data.vertices[data.indexes[i]];

								if (transparent)
								{
									//transparentVertices.Add(vert);
									transparentIndexes.Add(initialIndex + data.indexes[i]);
								}
								else
								{
									//	solidVertices.Add(vert);
									solidIndexes.Add(initialIndex + data.indexes[i]);
								}
							}


							//positions.TryAdd(new Vector3(x, (index1 * 16) + y, z),
							//	new ChunkMesh.EntryPosition(transparent, initialIndexIndex, data.indexes.Length));
						}
					}
				}
				//Log.Info($"Re-build took: {sw.ElapsedMilliseconds}ms - Mode: FULL CHUNK");
			}
			
		//	PositionCache = new Dictionary<Vector3, ChunkMesh.EntryPosition>(positions);
			return new ChunkMesh(solidVertices.ToArray(), transparentVertices.ToArray(),
				solidIndexes.ToArray(), transparentIndexes.ToArray());
		}*/

			public async Task<ChunkMesh> GenerateMeshes(IWorld world)
			{
				
            return null;
			}

			private class SectionEntry : IDisposable
			{
				public DynamicIndexBuffer SolidIndexBuffer { get; set; }
				public DynamicIndexBuffer TransparentIndexBuffer { get; set; }

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

			private SectionEntry[] SectionBuffers = new SectionEntry[16]
			{
				null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
			};

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

				for (var index = Sections.Length - 1; index >= 0; index--)
				{
					var section = Sections[index];
					if (section == null || section.IsEmpty())
					{
						if (X == 0 && Z == 0)
						{
							Log.Info($"Not updating cause null or empty: (x: {X} z: {Z}) {index} null: {section == null}");
						}

						continue;
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
								ChunkMesh mesh = GenerateSectionMesh(world, oldMeshCache, ref section, index,
									out var positions);
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
										SolidBuffer = new VertexBuffer(device,
											VertexPositionNormalTextureColor.VertexDeclaration, mesh.Vertices.Length,
											BufferUsage.WriteOnly),
										SolidIndexBuffer = new DynamicIndexBuffer(device,
											IndexElementSize.ThirtyTwoBits,
											mesh.SolidIndexes.Length, BufferUsage.WriteOnly),
										TransparentBuffer = new VertexBuffer(device,
											VertexPositionNormalTextureColor.VertexDeclaration,
											mesh.TransparentVertices.Length,
											BufferUsage.WriteOnly),
										TransparentIndexBuffer = new DynamicIndexBuffer(device,
											IndexElementSize.ThirtyTwoBits,
											mesh.TransparentIndexes.Length, BufferUsage.WriteOnly)
									};


									if (mesh.SolidIndexes.Length > 0)
									{
										buffer.SolidBuffer.SetData(mesh.Vertices);
										buffer.SolidIndexBuffer.SetData(mesh.SolidIndexes);
									}

									if (mesh.TransparentIndexes.Length > 0)
									{
										buffer.TransparentBuffer.SetData(mesh.TransparentVertices);
										buffer.TransparentIndexBuffer.SetData(mesh.TransparentIndexes);
									}

								}
								else
								{
									//lock (buffer._lock)
									{
										if (buffer.SolidBuffer.VertexCount != mesh.Vertices.Length)
										{
											var oldVerticeBuffer = buffer.SolidBuffer;
											buffer.SolidBuffer = RenewVertexBuffer(device, mesh.Vertices);
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
												RenewVertexBuffer(device, mesh.TransparentVertices);
											oldVerticeBuffer.Dispose();
										}
										else
										{
											//buffer.TransparentBuffer.SetData(mesh.TransparentVertices);
										}

										if (buffer.SolidIndexBuffer.IndexCount != mesh.SolidIndexes.Length)
										{
											var oldIndexBuffer = buffer.SolidIndexBuffer;
											buffer.SolidIndexBuffer = RenewIndexBuffer(device, mesh.SolidIndexes);
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
												RenewIndexBuffer(device, mesh.TransparentIndexes);
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
				ref ExtendedBlockStorage section, int yIndex,
				out IDictionary<Vector3, ChunkMesh.EntryPosition> outputPositions)
			{
				var scheduled = Scheduled;
				List<VertexPositionNormalTextureColor> solidVertices = new List<VertexPositionNormalTextureColor>();
				List<VertexPositionNormalTextureColor> transparentVertices =
					new List<VertexPositionNormalTextureColor>();
				var positions = new ConcurrentDictionary<Vector3, ChunkMesh.EntryPosition>();

				List<int> transparentIndexes = new List<int>();
				List<int> solidIndexes = new List<int>();

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
						if (wasScheduled || wasLightingScheduled || scheduled.HasFlag(ScheduleType.Full) 
						    || (scheduled.HasFlag(ScheduleType.Lighting) /*&& cachedMesh == null*/))
						{
							var data = CalculateBlockVertices(world, section,
								yIndex,
								x, y, z, out bool transparent);

							if (data.vertices.Length > 0)
							{
								int startVerticeIndex = transparent ? transparentVertices.Count : solidVertices.Count;
								foreach (var vert in data.vertices)
								{
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
						else if (cachedMesh != null)
						{
							if (cachedMesh.Positions.TryGetValue(vector, out ChunkMesh.EntryPosition position))
							{
								var cachedVertices = position.Transparent
									? cachedMesh.Mesh.TransparentVertices
									: cachedMesh.Mesh.Vertices;
								
								var cachedIndices = position.Transparent
									? cachedMesh.Mesh.TransparentIndexes
									: cachedMesh.Mesh.SolidIndexes;
								
								if (cachedIndices == null) continue;

								//int verticeStartIndex =
								//	position.Transparent ? transparentVertices.Count : solidVertices.Count;

								//var unique = cachedIndices
								//	.Skip(position.Index + 1).Take(position.Length).Distinct().ToArray();
								//int startIndex = position.Transparent ? transparentIndexes.Count : solidIndexes.Count;
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
										transparentIndexes.Add( o);
									}
									else
									{
										solidIndexes.Add( o);
									}
								}
								
								//TODO: Find a way to update just the color of the faces, without having to recalculate vertices
								//We could save what vertices belong to what faces i suppose?
								//We could also use a custom shader to light the blocks...

								/*int startIndex = position.Transparent ? transparentIndexes.Count : solidIndexes.Count;
								for (int i = 0; i < position.Length; i++)
								{
									if (position.Transparent)
									{
										transparentIndexes.Add(verticeStartIndex + i);
									}
									else
									{
										solidIndexes.Add(verticeStartIndex + i);
									}
								}*/

								positions.TryAdd(vector,
									new ChunkMesh.EntryPosition(position.Transparent, startIndex, position.Length));
							}
							else
							{
							//	Log.Warn($"Could not find position {scheduled} : {vector}");
							}
						}
					}



				}
				

				outputPositions = positions;
				return new ChunkMesh(solidVertices.ToArray(), transparentVertices.ToArray(), solidIndexes.ToArray(),
					transparentIndexes.ToArray());
			}

			private bool IsUpdateScheduled(Vector3 position, bool[] originalSchedule, bool[] originalLightingSchedule)
		{
			if (position.Y < 0 || position.Y > ChunkHeight) return false;
			if (position.X < 0 || position.X > ChunkWidth) return false;
			if (position.Z < 0 || position.Z > ChunkDepth) return false;

			var section = Sections[(int)position.Y >> 4];
			if (section == null) return false;

			var y = (int) position.Y;

            int index = (y - 16 * (y >> 4)) << 8 | (int) position.Z << 4 | (int) position.X;
			return (originalSchedule[index] || section.ScheduledUpdates[index]) 
			       || (originalLightingSchedule != null && (section.ScheduledSkylightUpdates[index] || originalLightingSchedule[index]));
		}

		private TimeSpan TotalElapsed = TimeSpan.Zero;
		private long Counter = 0;
		private (VertexPositionNormalTextureColor[] vertices,
			int[] indexes) CalculateBlockVertices(IWorld world, ExtendedBlockStorage chunk,
			int index, int x, int y, int z, out bool transparent)
		{
			IBlockState stateId = chunk.Get(x, y, z);
			//length = 0;
			transparent = false;
			//idx = 0;
			
			if (stateId == null)
			{
				Log.Warn($"State is null!");
				return ((new VertexPositionNormalTextureColor[0], new int[0]));
			}

			
			IBlock block = stateId.Block;// BlockFactory.GetBlock(stateId);

			if (!block.Renderable)
			{
				return ((new VertexPositionNormalTextureColor[0], new int[0]));
			}

			var blockPosition = new Vector3(x, y + (index * 16), z) + Position;
			
		//	Stopwatch verticeTimer = Stopwatch.StartNew();

			var vert = stateId.Model.GetVertices(world, blockPosition, block);

			transparent = block.Transparent;
			//length = vert.vertices.Length;

			//verticeTimer.Stop();
			//var elapsed = verticeTimer.Elapsed;
			//TotalElapsed += elapsed;

		/*	if (Interlocked.Increment(ref Counter) == (4096 * 2))
			{
				var c = Interlocked.Exchange(ref Counter, 0);
				var total = TotalElapsed;
				TotalElapsed = TimeSpan.Zero;
				
				//Log.Info($"Time wasted on vertice building for {c} blocks: {total}");
			}*/


            return vert;
		
			if (block.Transparent)
			{
				//idx = transparentVertices.Count;
				//transparentVertices.AddRange(vert.vertices);
				//solidIndexes.AddRange(vert.indexes);
			}
			else
			{
				//idx = solidVertices.Count;
				//solidVertices.AddRange(vert.vertices);
				//solidIndexes.AddRange(vert.indexes);
			}
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
			if (VertexBuffer != null)
			{
				if (!VertexBuffer.IsDisposed)
				{
					VertexBuffer.Dispose();
				}

				VertexBuffer = null;
			}

			/*if (TransparentVertexBuffer != null)
			{
				if (!TransparentVertexBuffer.IsDisposed)
				{
					TransparentVertexBuffer.Dispose();
				}

				TransparentVertexBuffer = null;
			}*/

			if (IndexBuffer != null)
			{
				if (!IndexBuffer.IsDisposed)
				{
					IndexBuffer.Dispose();
				}

				IndexBuffer = null;
			}

			if (TransparentIndexBuffer != null)
			{
				if (!TransparentIndexBuffer.IsDisposed)
				{
					TransparentIndexBuffer.Dispose();
				}

				TransparentIndexBuffer = null;
			}

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
		}

		public void CalculateSkylight()
		{
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					int light = 15;
					for (int y = 256 - 1; y > 0; --y)
					{
						var block = GetBlock(x, y, z);
						if (!block.Renderable) continue;

						if (block.Solid && !block.Transparent)
						{
							light = 0;
						}
						else
						{
							var lightOpacity = block.LightOpacity;
							if (lightOpacity == 0 && light != 15)
							{
								lightOpacity = 1;
							}

							light -= lightOpacity;
							//(int) Math.Round(prevLight *  (1D - block.LightOpacity));
						}

						if (light > 0)
						{
							SetSkyLight(x, y, z, (byte) light);
						}

					}
				}
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
							storage = new ExtendedBlockStorage(sectionY, readSkylight);
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
								storage = new ExtendedBlockStorage(sectionY, readSkylight);
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
