using System.IO.Compression;

namespace ResourcePackLib.Core.IO;

public class ZipFileSystemEntry : IFileSystemEntry
{
    protected ZipDirectoryRoot Root { get; }

    public string Path { get; }
    public string Name { get; }
    
    public string FullName
    {
        get => System.IO.Path.Join(Path, Name);
    }

    protected internal ZipFileSystemEntry(ZipDirectoryRoot zipDirectoryRoot, string relativePath)
    {
        Root = zipDirectoryRoot;

        var i = relativePath.LastIndexOfAny(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
        if (i <= 0)
        {
            Path = string.Empty;
            Name = relativePath;
        }
        else
        {
            Path = relativePath[..i];
            Name = relativePath[i..];
        }
    }


    public bool Exists() => Root.EntryExists(FullName);
}