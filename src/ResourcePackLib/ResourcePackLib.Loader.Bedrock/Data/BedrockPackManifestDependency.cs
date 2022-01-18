using System.Numerics;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockPackManifestDependency
{
    public Guid Uuid { get; set; }
    public Vector3 Version { get; set; }
}