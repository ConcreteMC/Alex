namespace ResourcePackLib.Core.IO;

public interface IFileSystemEntry
{
    internal static readonly Predicate<IFileSystemEntry> NoFilter = f => true;
    
    string Path { get; }
    
    string Name { get; }
    
    bool Exists();

    public string FullName
    {
        get => System.IO.Path.Join(Path, Name);
    }
}