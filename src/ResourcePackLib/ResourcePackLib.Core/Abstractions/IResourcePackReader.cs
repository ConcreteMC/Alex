using ResourcePackLib.Core.IO;

namespace ResourcePackLib.Core;

public interface IResourcePackReader
{
    Task<bool> CanLoadAsync(IDirectory dir);
    Task<ResourcePack> LoadAsync(IDirectory dir);
}