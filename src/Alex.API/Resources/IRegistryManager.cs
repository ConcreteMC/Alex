using System;

namespace Alex.API.Resources
{
    public interface IRegistryManager
    {
        IRegistry<TType> GetRegistry<TType>() where TType : class, IRegistryEntry<TType>;
        IRegistry GetRegistry(Type type);
        void AddRegistry<TType>(IRegistry<TType> registry) where TType : class, IRegistryEntry<TType>;
    }
}