using ResourcePackLib.Core;
using ResourcePackLib.Core.IO;

namespace ResourcePackLib.Loader.Bedrock;

public class BedrockResourcePackReader : IResourcePackReader
{
    public BedrockResourcePackReader()
    {
        
    }
    
    public async Task<bool> CanLoadAsync(IDirectory dir)
    {
        return dir["manifest.json"].Exists();
    }

    public async Task<ResourcePack> LoadAsync(IDirectory dir)
    {
        var pack = new BedrockResourcePackImpl(dir);

        
        
        return pack;
    }
}