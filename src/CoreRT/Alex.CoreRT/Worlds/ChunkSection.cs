using System.Linq;
using Alex.CoreRT.API.World;
using log4net;
using MiNET.Utils;

namespace Alex.CoreRT.Worlds
{
	public class ChunkSection : IChunk
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ChunkSection));

		private bool _isAllAir = true;

		public uint[] blocks = new uint[16 * 16 * 16];

		public NibbleArray blocklight = new NibbleArray(16 * 16 * 16);
		public NibbleArray skylight = new NibbleArray(16 * 16 * 16);

		private byte[] _cache;
		private bool _isDirty;
		private object _cacheSync = new object();

		public ChunkSection()
		{
			skylight.Data.Fill<byte>(0xff);
			blocks.Fill<uint>(0);
		}

		public bool IsDirty() => _isDirty;

		public bool IsAllAir()
		{
			if (_isDirty)
			{
				_isAllAir = blocks.All(b => b == 0);
				_isDirty = false;
			}
			return _isAllAir;
		}

		private static int GetIndex(int bx, int by, int bz)
		{
			return (bx << 8) + (bz << 4) + by;
		}

		public uint GetBlockState(int bx, int by, int bz)
		{
			return blocks[GetIndex(bx, by, bz)];
		}

		public void SetBlockState(int bx, int by, int bz, uint value)
		{
			blocks[GetIndex(bx, by, bz)] = value;
			if (value != 0 && _isAllAir)
			{
				_isAllAir = false;
			}
			else if (value == 0)
			{
				_isDirty = true;
			}
		}

		public byte GetBlocklight(int bx, int by, int bz)
		{
			return blocklight[GetIndex(bx, by, bz)];
		}

		public void SetBlocklight(int bx, int by, int bz, byte data)
		{
			blocklight[GetIndex(bx, by, bz)] = data;
		}

		public byte GetSkylight(int bx, int by, int bz)
		{
			return skylight[GetIndex(bx, by, bz)];
		}

		public void SetSkylight(int bx, int by, int bz, byte data)
		{
			skylight[GetIndex(bx, by, bz)] = data;
		}

		public object Clone()
		{
			ChunkSection cc = (ChunkSection)MemberwiseClone();

			cc.blocks = (uint[])blocks.Clone();
			//cc.metadata = (NibbleArray)metadata.Clone();
			cc.blocklight = (NibbleArray)blocklight.Clone();
			cc.skylight = (NibbleArray)skylight.Clone();

			if (_cache != null)
			{
				cc._cache = (byte[])_cache.Clone();
			}

			cc._cacheSync = new object();

			return cc;
		}
	}
}
