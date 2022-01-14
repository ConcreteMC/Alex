using System.Numerics;

namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockPackManifestHeader
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid Uuid { get; set; }
    public Vector3 Version { get; set; }
    public Vector3 MinEngineVersion { get; set; }
    public Vector3 BaseGameVersion { get; set; }
    public bool LockTemplateOptions { get; set; }
}