namespace ResourcePackLib.Core.IO;

public class ZipDirectory : ZipFileSystemEntry, IDirectory
{
    internal ZipDirectory(ZipDirectoryRoot root, string relativePath) : base(root, relativePath)
    {
        
    }

    public IFile GetFile(string name) => new ZipFile(Root, name);
    public IFileSystemEntry GetEntry(string name) => GetEntries(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)).SingleOrDefault() ?? new ZipFileSystemEntry(Root, name);

    public IFileSystemEntry[] GetEntries(Predicate<IFileSystemEntry> predicate)
    {
        predicate += GetDirectoryPredicate(FullName);
        
        return Root.GetEntries(predicate);
    }

    private static Predicate<IFileSystemEntry> GetRecursiveDirectoryPredicate(string path)
    {
        path = path.EndsWith('/') ? path : $"{path}/";
        return (f => f.FullName.StartsWith(path, StringComparison.OrdinalIgnoreCase));
    }
    
    private static Predicate<IFileSystemEntry> GetDirectoryPredicate(string path)
    {
        path = path.TrimEnd('/');
        return (f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
    }
}