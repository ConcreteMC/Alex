namespace ResourcePackLib.Loader.Bedrock.Data;

public class BedrockPackManifest
{
    public int FormatVersion { get; set; }
    
    public BedrockPackManifestHeader Header { get; set; }
    public BedrockPackManifestHeader Metadata { get; set; }
    public BedrockPackManifestModule[] Modules { get; set; }
    public BedrockPackManifestDependency[] Dependencies { get; set; }
    
}