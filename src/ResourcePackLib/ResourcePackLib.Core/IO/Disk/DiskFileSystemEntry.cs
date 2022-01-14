using System.IO.Enumeration;

namespace ResourcePackLib.Core.IO;

public class DiskFileSystemEntry : IFileSystemEntry
{
    private readonly FileSystemInfo _fsInfo;

    public string Path => _fsInfo.FullName[.._fsInfo.FullName.IndexOf(Name, StringComparison.Ordinal)];
    public string Name => _fsInfo.Name;

    public string FullName => System.IO.Path.Join(Path, Name);
    protected DiskFileSystemEntry(FileSystemInfo fsInfo)
    {
        _fsInfo = fsInfo;
    }
    
    public bool Exists() => _fsInfo.Exists;
}