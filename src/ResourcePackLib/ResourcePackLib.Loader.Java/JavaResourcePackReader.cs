using ResourcePackLib.Core;
using ResourcePackLib.Core.IO;

namespace ResourcePackLib.Loader.Java;

public class JavaResourcePackReader : IResourcePackReader
{
    public JavaResourcePackReader()
    {
        
    }
    
    public async Task<bool> CanLoadAsync(IDirectory dir)
    {
        return dir["pack.mcmeta"].Exists();
    }
}