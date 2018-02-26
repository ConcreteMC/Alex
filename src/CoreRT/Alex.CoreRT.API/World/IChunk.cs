using System;

namespace Alex.CoreRT.API.World
{
	public interface IChunk : ICloneable
	{
		bool IsDirty();
		bool IsAllAir();

		uint GetBlockState(int bx, int by, int bz);

		/*int GetBlock(int bx, int by, int bz);
		void SetBlock(int bx, int by, int bz, int bid);

		byte GetMetadata(int bx, int by, int bz);
		void SetMetadata(int bx, int by, int bz, byte data);*/

		byte GetBlocklight(int bx, int by, int bz);
		void SetBlocklight(int bx, int by, int bz, byte data);

		byte GetSkylight(int bx, int by, int bz);
		void SetSkylight(int bx, int by, int bz, byte data);
	}
}
