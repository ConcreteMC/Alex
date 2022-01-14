namespace ResourcePackLib.Core.IO;

public interface IFile : IFileSystemEntry
{
    internal static readonly Predicate<IFile> NoFilter = f => true;

    Stream OpenRead();
    
    long   Length   { get; }

    string Extension { get; }
    
    string FullName => System.IO.Path.Join(Path, string.IsNullOrEmpty(Extension) ? Name : $"{Name}.{Extension}");
}