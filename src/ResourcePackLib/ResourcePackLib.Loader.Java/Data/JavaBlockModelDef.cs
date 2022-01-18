using System.Numerics;
using Newtonsoft.Json;
using ResourcePackLib.Core.Data;
using ResourcePackLib.Core.Utils;

namespace ResourcePackLib.Loader.Java.Data;

public class JavaUVDef
{
    public int X1 { get; set; } = 0;
    public int Y1 { get; set; } = 0;
    public int X2 { get; set; } = 16;
    public int Y2 { get; set; } = 16;

    public JavaUVDef()
    {
    }

    public JavaUVDef(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }
}

public class JavaBlockModelDef
{
    public ResourceLocation Parent { get; set; }
    
    public List<JavaBlockModelDefElement> Elements { get; set; }
    
    public NamedResourceLocationCollection Textures { get; set; }
    
    /// <summary>
    /// Whether to use ambient occlusion (true - default), or not (false).
    /// </summary>
    public bool AmbientOcclusion { get; set; } = true;
}

public class JavaBlockModelDefElementFaceCollection : Dictionary<CubeFace, JavaBlockModelDefElementFace>
{
    public JavaBlockModelDefElementFace Down
    {
        get => this[CubeFace.Down];
        set => this[CubeFace.Down] = value;
    }

    public JavaBlockModelDefElementFace Up
    {
        get => this[CubeFace.Up];
        set => this[CubeFace.Up] = value;
    }

    public JavaBlockModelDefElementFace North
    {
        get => this[CubeFace.North];
        set => this[CubeFace.North] = value;
    }

    public JavaBlockModelDefElementFace South
    {
        get => this[CubeFace.South];
        set => this[CubeFace.South] = value;
    }

    public JavaBlockModelDefElementFace East
    {
        get => this[CubeFace.East];
        set => this[CubeFace.East] = value;
    }

    public JavaBlockModelDefElementFace West
    {
        get => this[CubeFace.West];
        set => this[CubeFace.West] = value;
    }
}

public class JavaBlockModelDefElementFace
{
    /// <summary>
    /// Defines the area of the texture to use according to the scheme [x1, y1, x2, y2]. If unset, it defaults to values equal to xyz position of the element. The texture behavior will be inconsistent if UV extends below 0 or above 16. If the numbers of x1 and x2 are swapped (e.g. from 0, 0, 16, 16 to 16, 0, 0, 16), the texture will be flipped. UV is optional, and if not supplied it will automatically generate based on the element's position.
    /// </summary>
    public JavaUVDef Uv { get; set; }

    /// <summary>
    /// Specifies the texture in form of the texture variable prepended with a #.
    /// </summary>
    public ResourceLocation Texture { get; set; }

    /// <summary>
    /// Specifies whether a face does not need to be rendered when there is a block touching it in the specified position. The position can be: down, up, north, south, west, or east. It will also determine which side of the block to use the light level from for lighting the face, and if unset, defaults to the side.
    /// </summary>
    public CubeFace CullFace { get; set; }

    /// <summary>
    /// Rotates the texture by the specified number of degrees. Can be 0, 90, 180, or 270. Defaults to 0. Rotation does not affect which part of the texture is used. Instead, it amounts to permutation of the selected texture vertexes (selected implicitly, or explicitly though uv).
    /// </summary>
    public int Rotation { get; set; } = 0;

    /// <summary>
    /// Determines whether to tint the texture using a hardcoded tint index. The default is not using the tint, and any number causes it to use tint. Note that only certain blocks have a tint index, all others will be unaffected.
    /// </summary>
    public int? TintIndex { get; set; } = null;
}

public class JavaBlockModelDefElement
{
    public Vector3 From { get; set; }
    public Vector3 To { get; set; }

    public JavaBlockModelDefElementFaceCollection Faces { get; set; }

    [JsonProperty("__comment")] public string Comment { get; set; }
}