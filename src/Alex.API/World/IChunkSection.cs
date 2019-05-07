using Alex.API.Blocks.State;

namespace Alex.API.World
{
	public interface IChunkSection
	{
		IBlockState GetBlockState(int x, int y, int z);
		void SetBlockState(int x, int y, int z, IBlockState state);
	}
}
