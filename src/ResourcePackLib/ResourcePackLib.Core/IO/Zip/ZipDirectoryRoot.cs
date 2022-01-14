using System.IO.Compression;

namespace ResourcePackLib.Core.IO;

public class ZipDirectoryRoot : DiskFile
{
    private readonly FileInfo _zipFileInfo;
    private List<ZipFileSystemEntry>? _allEntries;
    
    public ZipDirectoryRoot(FileInfo fileInfo) : base(fileInfo)
    {
    }

    private bool EnsureEntriesLoaded()
    {
        if (!Exists()) return false;
        LoadEntries();
        
        return true;
    }

    private void LoadEntries()
    {
        if(_allEntries != null) return;
        
        using var fs = OpenRead();
        using var zip = new ZipArchive(fs, ZipArchiveMode.Read, true);
        _allEntries = zip.Entries.Select<ZipArchiveEntry, ZipFileSystemEntry>(e => 
                e.FullName.EndsWith('/') 
                    ? new ZipDirectory(this, e.FullName) 
                    : new ZipFile(this, e))
            .ToList();
    }

    internal ZipFileSystemEntry[] GetEntries(Predicate<IFileSystemEntry> predicate)
    {
        if (!EnsureEntriesLoaded())
            throw new InvalidOperationException();

        return _allEntries.Where(f => predicate(f)).ToArray();
    }
    
    internal bool EntryExists(string path)
    {
        if(!EnsureEntriesLoaded()) return false;
        
        if (path.EndsWith('/'))
        {
            return _allEntries.Any(f => f.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase));
        }

        return _allEntries.Any(f => string.Equals(f.FullName, path, StringComparison.OrdinalIgnoreCase));
    }
    
    public static explicit operator ZipDirectory(ZipDirectoryRoot root)
    {
        return new ZipDirectory(root, string.Empty);
    }
}