namespace ResourcePackLib.Core.IO;

public class DiskFile : DiskFileSystemEntry, IFile
{
    internal DiskFile(FileInfo fileInfo) : base(fileInfo)
    {
    }

    public Stream OpenRead()
    {
        return File.OpenRead(this.FullName);
    }

    public long Length { get; }
    public string Extension { get; }
}