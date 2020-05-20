using System;
using System.Collections.Generic;

namespace Alex.API.Resources
{
    public interface IRegistry
    {
        ResourceLocation RegistryName { get; }
        Type RegistryType { get; }
    }
    
    public interface IRegistry<TEntry> : IRegistry, IEnumerable<IRegistryEntry<TEntry>> where TEntry : class//, IRegistryEntry<TType>
    {
        void Register(Func<IRegistryEntry<TEntry>> entry);
        void Register(ResourceLocation location, IRegistryEntry<TEntry> entry);
        void RegisterRange(params Func<IRegistryEntry<TEntry>>[] entries);

        bool ContainsKey(ResourceLocation location);
        bool TryGet(ResourceLocation location, out IRegistryEntry<TEntry> value);

        IRegistryEntry<TEntry> Get(ResourceLocation location);
        void Set(ResourceLocation location, Func<IRegistryEntry<TEntry>> entry);
    }
}