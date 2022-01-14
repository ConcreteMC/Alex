namespace ResourcePackLib.Core.IO;

public interface IDirectory : IFileSystemEntry
{
    internal static readonly Predicate<IDirectory> NoFilter = f => true;

    #region Files

    IFile GetFile(string name);
    
    IFile[] GetFiles() => GetFiles(IFile.NoFilter);
    IFile[] GetFiles(Predicate<IFile> predicate) => GetEntries(f => f is IFile ff && predicate(ff)).Cast<IFile>().ToArray();

    
    #endregion Files

    #region Directories

    IDirectory[] GetDirectories() => GetDirectories(NoFilter);
    IDirectory[] GetDirectories(Predicate<IDirectory> predicate) => GetEntries(f => f is IDirectory d && predicate(d)).Cast<IDirectory>().ToArray();

    #endregion Directories
    
    #region Entries

    IFileSystemEntry this[string name] => GetEntry(name);
    IFileSystemEntry GetEntry(string name);
    
    IFileSystemEntry[] GetEntries() => GetEntries(IFileSystemEntry.NoFilter);
    IFileSystemEntry[] GetEntries(Predicate<IFileSystemEntry> predicate);
    
    #endregion
}