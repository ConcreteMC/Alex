using Alex.Interfaces.Resources;

namespace Alex.Common.Resources
{
	public interface IRegistryEntry<TType> where TType : class
	{
		ResourceLocation Location { get; }

		IRegistryEntry<TType> WithLocation(ResourceLocation location);

		TType Value { get; }
	}
}