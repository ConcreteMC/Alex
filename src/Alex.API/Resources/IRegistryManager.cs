using System;

namespace Alex.API.Resources
{
    public interface IRegistryManager
    {
        IRegistry<TType> GetRegistry<TType>() where TType : class;
        IRegistry GetRegistry(Type type);
        void AddRegistry<TType>(IRegistry<TType> registry) where TType : class, IRegistryEntry<TType>;
        void AddRegistry<TType, TEntry>(IRegistry<TEntry> registry) where TType : class, IRegistryEntry<TEntry> where TEntry : class;
    }
}