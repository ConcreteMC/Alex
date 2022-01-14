using System.Numerics;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockPackManifestModule
{
    public string Description { get; set; }
    public BedrockPackManifestModuleType Type { get; set; }
    public Guid Uuid { get; set; }
    public Vector3 Version { get; set; }
}