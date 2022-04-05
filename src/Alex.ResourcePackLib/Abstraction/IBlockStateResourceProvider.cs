using Alex.Interfaces.Resources;
using Alex.ResourcePackLib.Json.BlockStates;

namespace Alex.ResourcePackLib.Abstraction
{
	public interface IBlockStateResourceProvider
	{
		bool TryGetBlockState(ResourceLocation modelName, out BlockStateResource stateResource);
	}
}