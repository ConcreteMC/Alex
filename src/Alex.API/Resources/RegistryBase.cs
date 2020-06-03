using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace Alex.API.Resources
{
    public class RegistryBase<TEntry> : IRegistry<TEntry> where TEntry : class //, IRegistryEntry<TType>
    {
        protected ConcurrentDictionary<ResourceLocation, Func<IRegistryEntry<TEntry>>> Entries { get; }
        public ResourceLocation RegistryName { get; }
        public Type RegistryType => typeof(TEntry);

        public RegistryBase(string registryName)
        {
            RegistryName = registryName;
            Entries = new ConcurrentDictionary<ResourceLocation, Func<IRegistryEntry<TEntry>>>();
        }

        public void Set(ResourceLocation location, Func<IRegistryEntry<TEntry>> entry)
        {
            Entries.AddOrUpdate(location, entry, (resourceLocation, func) => entry);
        }

        public void Register(Func<IRegistryEntry<TEntry>> entry)
        {
            if (!Entries.TryAdd(entry().Location, entry))
                throw new DuplicateNameException("An item with this location has already been registered!");
        }

        public void Register(ResourceLocation location, Func<IRegistryEntry<TEntry>> entry)
        {
            if (!Entries.TryAdd(location, entry))
                throw new DuplicateNameException("An item with this location has already been registered!");
        }

        public void Register(ResourceLocation location, IRegistryEntry<TEntry> entry)
        {
            if (!Entries.TryAdd(location, () => entry))
                throw new DuplicateNameException("An item with this location has already been registered!");
        }

        public void Register(IRegistryEntry<TEntry> entry)
        {
            if (!Entries.TryAdd(entry.Location, () => entry))
                throw new DuplicateNameException("An item with this location has already been registered!");
        }

        public virtual void RegisterRange(params Func<IRegistryEntry<TEntry>>[] entries)
        {
            foreach (var entry in entries)
            {
                Register(entry);
            }
        }

        public virtual bool ContainsKey(ResourceLocation location)
        {
            return Entries.ContainsKey(location);
        }

        public virtual bool TryGet(ResourceLocation location, out IRegistryEntry<TEntry> value)
        {
            if (Entries.TryGetValue(location, out var factory))
            {
                value = factory();
                return true;
            }

            value = default;
            return false;
        }

        public virtual IRegistryEntry<TEntry> Get(ResourceLocation location)
        {
            if (TryGet(location, out IRegistryEntry<TEntry> value))
                return value;

            throw new KeyNotFoundException("Could not find a registry item with specified location!");
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IRegistryEntry<TEntry>> GetEnumerator()
        {
            var entries = Entries.ToArray();
            foreach (var entry in entries)
            {
                yield return entry.Value();
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}