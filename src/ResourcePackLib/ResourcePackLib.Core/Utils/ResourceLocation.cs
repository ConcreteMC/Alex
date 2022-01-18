using System.Diagnostics;

namespace ResourcePackLib.Core.Utils;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ResourceLocation : IEquatable<ResourceLocation>
{
    private string DebuggerDisplay => ToString();

    public const string DefaultNamespace = "minecraft";
    public string Namespace { get; }
    public string Path { get; }

    public int Length => Namespace.Length + Path.Length;

    public bool IsReference { get; }

    public ResourceLocation(string key)
    {
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        var s = key.Split(':', 1);
        if (s.Length == 2)
        {
            Namespace = s[0];
            Path = s[1];
        }
        else
        {
            Namespace = DefaultNamespace;
            Path = s[0];
        }

        IsReference = Path.StartsWith("#");
    }

    public ResourceLocation(string @namespace, string path)
    {
        Namespace = @namespace;
        Path = path;
    }


    public static implicit operator ResourceLocation(string input)
    {
        return new ResourceLocation(input);
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        return $"{Namespace}:{Path}";
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
    public bool Equals(ResourceLocation other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return GetHashCode() == other.GetHashCode();
    }


    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((ResourceLocation)obj);
    }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Namespace, Path);
    }
}