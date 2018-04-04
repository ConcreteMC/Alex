using Alex.API.Blocks.State;
using Alex.API.Utils;

namespace Alex.API.World
{
	public interface IBlockStatePalette
	{
		uint IdFor(IBlockState state);
		IBlockState GetBlockState(uint indexKey);
		void Read(IMinecraftStream ms);
	}
}
