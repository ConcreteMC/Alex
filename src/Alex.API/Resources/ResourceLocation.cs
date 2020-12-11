using System;
using System.Threading;

namespace Alex.API.Resources
{
    public class ResourceLocation : IEquatable<ResourceLocation>
    {
        public const string DefaultNamespace = "minecraft";
        public string Namespace { get; }
        public string Path { get; }

        public ResourceLocation(string key) : this(key.Contains(':') ? key.Substring(0, key.IndexOf(':')) : DefaultNamespace,
            key.Contains(':') ? key.Substring(key.IndexOf(':') + 1) : key)
        {

        }

        public ResourceLocation(string @namespace, string path)
        {
            Namespace = @namespace;
            Path = path;

           // _hashCode = GetUniqueId();
        }

        public int Length => Namespace.Length + Path.Length;
        
        public static implicit operator ResourceLocation(string input)
        {
            return new ResourceLocation(input);
        }

        public static bool operator ==(ResourceLocation a, ResourceLocation b)
        {
            if (a.ToString().Equals(b.ToString(), StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        public static bool operator !=(ResourceLocation a, ResourceLocation b)
        {
            return !(a == b);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Namespace}:{Path}";
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
      /*  public override bool Equals(object obj)
        {
            if (obj is ResourceLocation b)
            {
                return this == b;
            }

            if (obj is string str)
            {
                return ToString().Equals(str, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }*/

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(ResourceLocation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Namespace, other.Namespace, StringComparison.InvariantCultureIgnoreCase) && string.Equals(Path, other.Path, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ResourceLocation) obj);
        }
        
        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < Path.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ Path[i];
                    if (i == Path.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ Path[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        private static int _uniqueIdCounter = int.MinValue;

        private static int GetUniqueId()
        {
            return Interlocked.Increment(ref _uniqueIdCounter);
        }
    }
}