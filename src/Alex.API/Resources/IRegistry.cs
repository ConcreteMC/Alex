using System;
using System.Collections.Generic;

namespace Alex.API.Resources
{
    public interface IRegistry
    {
        ResourceLocation RegistryName { get; }
        Type RegistryType { get; }
    }
    
    public interface IRegistry<TType> : IRegistry, IEnumerable<IRegistryEntry<TType>> where TType : class, IRegistryEntry<TType>
    {
        void Register(Func<IRegistryEntry<TType>> entry);
        void RegisterRange(params Func<IRegistryEntry<TType>>[] entries);

        bool ContainsKey(ResourceLocation location);
        bool TryGet(ResourceLocation location, out IRegistryEntry<TType> value);

        IRegistryEntry<TType> Get(ResourceLocation location);
    }
}