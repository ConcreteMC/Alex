using System.IO.Compression;

namespace ResourcePackLib.Core.IO;

public class ZipFile : ZipFileSystemEntry, IFile
{

    public long Length { get; }
    public string Extension { get; }

    internal ZipFile(ZipDirectoryRoot zipDirectoryRoot, string fullPath) : base(zipDirectoryRoot, fullPath)
    {
        
    }
    
    internal ZipFile(ZipDirectoryRoot zipDirectoryRoot, ZipArchiveEntry entry) : this(zipDirectoryRoot, entry.FullName)
    {
    }

    public Stream OpenRead()
    {
        throw new NotImplementedException();
    }
}