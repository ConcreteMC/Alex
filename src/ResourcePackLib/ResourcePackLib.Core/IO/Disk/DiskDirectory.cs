namespace ResourcePackLib.Core.IO;

public class DiskDirectory : DiskFileSystemEntry, IDirectory
{
    private readonly DirectoryInfo _dirInfo;

    internal DiskDirectory(DirectoryInfo dirInfo) : base(dirInfo)
    {
        _dirInfo = dirInfo;
    }

    public IFile GetFile(string name)
    {
        var fileInfo = new FileInfo(System.IO.Path.Join(_dirInfo.FullName, name));
        return new DiskFile(fileInfo);
    }

    public IFile[] GetFiles(Predicate<IFile> predicate)
    {
        throw new NotImplementedException();
    }

    public IDirectory[] GetDirectories(Predicate<IDirectory> predicate)
    {
        throw new NotImplementedException();
    }

    public IFileSystemEntry GetEntry(string name)
    {
        throw new NotImplementedException();
    }

    public IFileSystemEntry[] GetEntries(Predicate<IFileSystemEntry> predicate)
    {
        return _dirInfo.GetFileSystemInfos()
            .Select<FileSystemInfo, IFileSystemEntry>(x => x switch
            {
                DirectoryInfo d => new DiskDirectory(d),
                FileInfo f when string.Equals(f.Extension, "zip", StringComparison.OrdinalIgnoreCase) => new ZipDirectoryRoot(f),
                FileInfo f => new DiskFile(f),
                _ => throw new InvalidOperationException()
            }).ToArray();
    }
}