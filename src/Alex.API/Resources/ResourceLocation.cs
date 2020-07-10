using System;

namespace Alex.API.Resources
{
    public class ResourceLocation : IEquatable<ResourceLocation>
    {
        public string Namespace { get; private set; }
        public string Path { get; private set; }

        public ResourceLocation(string key) : this(key.Contains(':') ? key.Substring(0, key.IndexOf(':')) : "minecraft",
            key.Contains(':') ? key.Substring(key.IndexOf(':')) : key)
        {

        }

        public ResourceLocation(string @namespace, string path)
        {
            Namespace = @namespace;
            Path = path;
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
                return ((Namespace != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Namespace) : 0) * 397) ^ (Path != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path) : 0);
            }
        }
    }
}