namespace Alex.API.Resources
{
    public interface IRegistryEntry<TType> where TType : class
    {
        ResourceLocation Location { get; }
        IRegistryEntry<TType> WithLocation(ResourceLocation location);
        
        TType Value { get; }
    }
}