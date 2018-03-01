using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.API.World;
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

		public ChunkSection[] Chunks = ArrayOf<ChunkSection>.Create(16);
		public byte[] BiomeId = ArrayOf<byte>.Create(256, 1);
		public short[] Height = new short[256];

		public VertexBuffer VertexBuffer { get; set; } = null;
		public VertexBuffer TransparentVertexBuffer { get; set; } = null;
		public object VertexLock { get; set; } = new object();
		public object UpdateLock { get; set; } = new object();
		public bool Scheduled { get; set; } = false;

		public ChunkColumn()
		{
			IsDirty = false;
		}

		private void SetDirty()
		{
			IsDirty = true;
			NeedSave = true;
		}

		public IBlock GetBlock(int bx, int by, int bz)
		{
			ChunkSection chunk = Chunks[by >> 4];
			return BlockFactory.GetBlock(chunk.GetBlockState(bx, by - 16 * (by >> 4), bz));
		}

		public void SetBlock(int bx, int by, int bz, IBlock block)
		{
			ChunkSection chunk = Chunks[by >> 4];
			chunk.SetBlockState(bx, by - 16 * (by >> 4), bz, block.BlockStateID);
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
			ChunkSection chunk = Chunks[by >> 4];
			return chunk.GetBlocklight(bx, by - 16 * (by >> 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			ChunkSection chunk = Chunks[by >> 4];
			chunk.SetBlocklight(bx, by - 16 * (by >> 4), bz, data);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			ChunkSection chunk = Chunks[by >> 4];
			return chunk.GetSkylight(bx, by - 16 * (by >> 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			ChunkSection chunk = Chunks[by >> 4];
			chunk.SetSkylight(bx, by - 16 * (by >> 4), bz, data);
		}

		private Vector3 Position => new Vector3(X * 16, 0, Z*16);
		public void GenerateMeshes(IWorld world, out Mesh mesh, out Mesh transparentMesh)
		{
			Vector3 pos = Position;
			var solidVertices = new List<VertexPositionNormalTextureColor>();
			var transparentVertices = new List<VertexPositionNormalTextureColor>();

			for (var index = 0; index < Chunks.Length; index++)
			{
				var chunk = Chunks[index];
				if (chunk.IsAllAir()) continue;

				for (var x = 0; x < ChunkWidth; x++)
				for (var z = 0; z < ChunkDepth; z++)
				for (var y = 0; y < 16; y++)
				{
					var stateId = chunk.GetBlockState(x, y, z);
					var block = BlockFactory.GetBlock(stateId);

					if (block.BlockId == 0 || !block.Renderable) continue;

					//TODO: Do lighting in here?

					var vert = block.GetVertices(new Vector3(x, y + (index * 16), z) + pos, world);
					if (block.Transparent)
					{
						transparentVertices.AddRange(vert);
					}
					else
					{
						solidVertices.AddRange(vert);
					}
				}
			}

			mesh = new Mesh(solidVertices.ToArray());
			transparentMesh = new Mesh(transparentVertices.ToArray());
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
	}
}
