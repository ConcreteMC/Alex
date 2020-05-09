using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace Alex.API.Resources
{
    public class RegistryManager : IRegistryManager
    {
        private Dictionary<ResourceLocation, IRegistry> Registries { get; }
        private Dictionary<Type, IRegistry> TypeToRegistry { get; }
        
        public RegistryManager()
        {
            Registries = new Dictionary<ResourceLocation, IRegistry>();
            TypeToRegistry = new Dictionary<Type, IRegistry>();
        }

        public IRegistry<TType> GetRegistry<TType>() where TType : class
        {
            return (IRegistry<TType>) GetRegistry(typeof(TType));
        }

        public IRegistry GetRegistry(Type type)
        {
            return TypeToRegistry[type];
        }

        public void AddRegistry<TType>(IRegistry<TType> registry) where TType : class, IRegistryEntry<TType>
        {
            var type = registry.RegistryType;
            var location = registry.RegistryName;
            
            if (Registries.ContainsKey(location))
                throw new DuplicateNameException();
            
            if (TypeToRegistry.ContainsKey(type))
                throw new Exception("A registry for this type already registered!");
            
            Registries.Add(location, registry);
            TypeToRegistry.Add(type, registry);
        }

        public void AddRegistry<TType, TEntry>(IRegistry<TEntry> registry) where TType : class, IRegistryEntry<TEntry> where TEntry : class
        {
            var type = typeof(TEntry);
            var location = registry.RegistryName;
            
            if (Registries.ContainsKey(location))
                throw new DuplicateNameException();
            
            if (TypeToRegistry.ContainsKey(type))
                throw new Exception("A registry for this type already registered!");
            
            Registries.Add(location, registry);
            TypeToRegistry.Add(type, registry);
        }
    }
}