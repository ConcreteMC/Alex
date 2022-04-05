using Alex.Interfaces.Resources;
using Alex.ResourcePackLib.Json.Models;

namespace Alex.ResourcePackLib.Abstraction
{
	public interface IBlockModelProvider
	{
		bool TryGetBlockModel(ResourceLocation resourceLocation, out ResourcePackModelBase model);
	}

	public interface IItemModelProvider
	{
		bool TryGetItemModel(ResourceLocation resourceLocation, out ResourcePackModelBase model);
	}
}