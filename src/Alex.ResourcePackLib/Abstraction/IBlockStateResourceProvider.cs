using Alex.ResourcePackLib.Json.BlockStates;

namespace Alex.ResourcePackLib.Abstraction
{
	public interface IBlockStateResourceProvider
	{
		bool TryGetBlockState(string modelName, out BlockStateResource stateResource);
	}
}