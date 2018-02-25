using Alex.Blocks;
using log4net;
using MiNET.Worlds;

namespace Alex.Worlds
{
	public class ChunkColumn
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ChunkColumn));

		public bool isAllAir = false;
		public bool isNew = true;

		public int x;
		public int z;

		public ChunkSection[] chunks = ArrayOf<ChunkSection>.Create(16);
		public byte[] biomeId = ArrayOf<byte>.Create(256, 1);
		public short[] height = new short[256];

		private byte[] _cache;
		public bool isDirty;
		public bool IsLoaded = false;
		public bool NeedSave = false;

		private object _cacheSync = new object();

		public ChunkColumn()
		{
			isDirty = false;
		}

		private void SetDirty()
		{
			_cache = null;
			isDirty = true;
			NeedSave = true;
		}

		public Block GetBlock(int bx, int by, int bz)
		{
			ChunkSection chunk = chunks[by >> 4];
			return BlockFactory.GetBlock(chunk.GetBlockState(bx, by - 16 * (by >> 4), bz));
		}

		public void SetBlock(int bx, int by, int bz, Block block)
		{
			ChunkSection chunk = chunks[by >> 4];
			chunk.SetBlockState(bx, by - 16 * (by >> 4), bz, block.BlockStateID);
			SetDirty();
		}

		public void SetHeight(int bx, int bz, short h)
		{
			height[((bz << 4) + (bx))] = h;
			SetDirty();
		}

		public byte GetHeight(int bx, int bz)
		{
			return (byte)height[((bz << 4) + (bx))];
		}

		public void SetBiome(int bx, int bz, byte biome)
		{
			biomeId[(bz << 4) + (bx)] = biome;
			SetDirty();
		}

		public byte GetBiome(int bx, int bz)
		{
			return biomeId[(bz << 4) + (bx)];
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			ChunkSection chunk = chunks[by >> 4];
			return chunk.GetBlocklight(bx, by - 16 * (by >> 4), bz);
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			ChunkSection chunk = chunks[by >> 4];
			chunk.SetBlocklight(bx, by - 16 * (by >> 4), bz, data);
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			ChunkSection chunk = chunks[by >> 4];
			return chunk.GetSkylight(bx, by - 16 * (by >> 4), bz);
		}

		public void SetSkyLight(int bx, int by, int bz, byte data)
		{
			ChunkSection chunk = chunks[by >> 4];
			chunk.SetSkylight(bx, by - 16 * (by >> 4), bz, data);
		}
	}
}
