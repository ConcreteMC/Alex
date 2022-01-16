using ResourcePackLib.Core.IO;
using ResourcePackLib.Core.Utils;

namespace ResourcePackLib.Core;

public class ResourcePackDescription
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ResourcePackIndex
{
    public ResourceLocationCollection Music { get; set; }

    public ResourceLocationCollection Sounds { get; set; }

    public ResourceLocationCollection Textures { get; set; }

    public ResourceLocationCollection Models { get; set; }

    public ResourceLocationCollection Locales { get; set; }

    public ResourceLocationCollection Particles { get; set; }
}

public class ResourcePack
{
    public ResourcePackIndex Index { get; } = new ResourcePackIndex();

    public IDirectory Directory { get; }

    public ResourcePack(IDirectory directory)
    {
        Directory = directory;
    }

    // public bool Contains(ResourceLocation resourceLocation)
    // {
    //     
    // }
    //
    // public bool TryGet<T>(ResourceLocation resourceLocation, out T resource)
    // {
    //     
    // }
}