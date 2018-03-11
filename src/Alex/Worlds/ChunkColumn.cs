using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Blocks.State;
using Alex.API.Graphics;
using Alex.API.World;
using Alex.Blocks;
using Alex.Blocks.Storage;
using fNbt.Tags;
using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;

namespace Alex.Worlds
{
	public class ChunkColumn : IChunkColumn
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ChunkColumn));

		public const int ChunkHeight = 256;
		public const int ChunkWidth = 16;
		public const int ChunkDepth = 16;

		public int X;
		public int Z;

		public bool IsAllAir = false;
		public bool IsNew = true;
		public bool IsLoaded = false;
		public bool NeedSave = false;
		public bool IsDirty { get; set; }

		public ExtendedBlockStorage[] Chunks = new ExtendedBlockStorage[16];
		public byte[] BiomeId = ArrayOf<byte>.Create(256, 1);
		public short[] Height = new short[256];

		public VertexBuffer VertexBuffer { get; set; } = null;
		public VertexBuffer TransparentVertexBuffer { get; set; } = null;
		public object VertexLock { get; set; } = new object();
		public object UpdateLock { get; set; } = new object();
		public ScheduleType Scheduled { get; set; } = ScheduleType.Unscheduled;

		public ChunkColumn()
		{
			var air = BlockFactory.GetBlockState(0);
			IsDirty = true;
			for (int i = 0; i < Chunks.Length; i++)
			{
				var b = new ExtendedBlockStorage(i, true);
				for (int x = 0; x < 16; x++)
				{
					for (int z = 0; z < 16; z++)
					{
						for (int y = 0; y < 16; y++)
						{
							b.Set(x,y,z, air);
						}
					}
				}
				Chunks[i] = b;
			}
		}

		private void SetDirty()
		{
			IsDirty = true;
			NeedSave = true;
		}

		public void SetBlockState(int x, int y, int z, IBlockState blockState)
		{
			Chunks[y >> 4].Set(x, y - 16 * (y >> 4), z, blockState);
			SetDirty();
		}

		public IBlockState GetBlockState(int bx, int by, int bz)
		{
			return Chunks[by >> 4].Get(bx, by - 16 * (by >> 4), bz);
		}

		public IBlock GetBlock(int bx, int by, int bz)
		{
			var bs = GetBlockState(bx, by, bz);

			if (bs == null) return new Air();

			return bs.GetBlock();
		}

		public void SetBlock(int bx, int by, int bz, IBlock block)
		{
			Chunks[by >> 4].Set(bx, by - 16 * (by >> 4), bz, block.BlockState);
			SetDirty();
		}

		public void SetHeight(int bx, int bz, short h)
		{
			Height[((bz << 4) + (bx))] = h;
			SetDirty();
		}

		public byte GetHeight(int bx, int bz)
		{
			return (byte)Height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int bz, byte biome)
		{
			BiomeId[(bz << 4) + (bx)] = biome;
			SetDirty();
		}

		public byte GetBiome(int bx, int bz)
		{
			return BiomeId[(bz << 4) + (bx)];
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			return (byte) Chunks[@by >> 4].GetExtBlocklightValue(bx, @by - 16 * (@by >> 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			Chunks[@by >> 4].SetExtBlocklightValue(bx, @by - 16 * (@by >> 4), bz, data);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			return Chunks[@by >> 4].GetExtSkylightValue(bx, @by - 16 * (@by >> 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			Chunks[@by >> 4].SetExtSkylightValue(bx, @by - 16 * (@by >> 4), bz, data);
		}

		private IReadOnlyDictionary<Vector3, ChunkMesh.EntryPosition> PositionCache { get; set; } = null;
		private Vector3 Position => new Vector3(X * 16, 0, Z*16);

		public NbtCompound[] Entities { get; internal set; }

		public void GenerateMeshes(IWorld world, out ChunkMesh mesh)
		{
			List<ChunkMesh.Entry> solidVertices = new List<ChunkMesh.Entry>();
			List<ChunkMesh.Entry> transparentVertices = new List<ChunkMesh.Entry>();

			//if (Scheduled == ScheduleType.Full || PositionCache == null)
			{
				for (var index = 0; index < Chunks.Length; index++)
				{
					var chunk = Chunks[index];
					if (chunk.IsEmpty()) continue;

					for (var x = 0; x < ChunkWidth; x++)
					for (var z = 0; z < ChunkDepth; z++)
					for (var y = 0; y < 16; y++)
					{
						Update(world, chunk,
							index,
							x, y, z,
							solidVertices,
							transparentVertices);
					}
				}
			}
		/*	else if (Scheduled == ScheduleType.Border)
			{
				for (var index = 0; index < Chunks.Length; index++)
				{
					var chunk = Chunks[index];
					if (chunk.IsAllAir()) continue;

					for (int x = 0; x < ChunkWidth; x++)
					{
						for (var y = 0; y < 16; y++)
						{
							Update(world, chunk,
								index,
								x, y, 0,
								solidVertices,
								transparentVertices);

							Update(world, chunk,
								index,
								x, y, 15,
								solidVertices,
								transparentVertices);

							Update(world, chunk,
								index,
								0, y, x,
								solidVertices,
								transparentVertices);

							Update(world, chunk,
								index,
								15, y, x,
								solidVertices,
								transparentVertices);
						}
					}
				}
			}
			else if (Scheduled == ScheduleType.Scheduled)
			{

			}*/

			mesh = new ChunkMesh(solidVertices.ToArray(), transparentVertices.ToArray());
		//	PositionCache = mesh.EntryPositions;
		}

		private void Update(IWorld world, ExtendedBlockStorage chunk,
			int index, int x, int y, int z,
			List<ChunkMesh.Entry> solidVertices,
			List<ChunkMesh.Entry> transparentVertices)
		{
			var stateId = chunk.Get(x, y, z);

			if (stateId == null)
			{
				Log.Warn($"State is null!");
				return;
			}

			IBlock block = stateId.GetBlock();// BlockFactory.GetBlock(stateId);

			if (block.BlockId == 0 || !block.Renderable) return;

			var blockPosition = new Vector3(x, y + (index * 16), z) + Position;

			var vert = block.GetVertices(blockPosition, world);
			var result = new ChunkMesh.Entry(vert, blockPosition);

			if (block.Transparent)
			{
				transparentVertices.Add(result);
				//transparentVertices.AddRange(vert);
			}
			else
			{
				solidVertices.Add(result);
				//solidVertices.AddRange(vert);
			}
		}

		public int GetHeighest()
		{
			return Height.Max();
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

			if (TransparentVertexBuffer != null)
			{
				if (!TransparentVertexBuffer.IsDisposed)
				{
					TransparentVertexBuffer.Dispose();
				}

				TransparentVertexBuffer = null;
			}
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
	}
}
