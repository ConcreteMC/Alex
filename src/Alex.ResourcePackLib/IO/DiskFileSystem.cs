using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Alex.ResourcePackLib.IO.Abstract;

namespace Alex.ResourcePackLib.IO
{
	public class DiskFileSystem : IFilesystem
	{
		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public IReadOnlyCollection<IFile> Entries { get; }

		/// <inheritdoc />
		public bool CanReadAsync => true;

		private string Root { get; }
		public DiskFileSystem(string path)
		{
			Root = path;
			Name = Path.GetDirectoryName(path);
			
			List<IFile> entries = new List<IFile>();
			foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
			{
				entries.Add(new FileSystemEntry(new FileInfo(file), Path.GetRelativePath(path, file)));
			}

			Entries = new ReadOnlyCollection<IFile>(entries);
		}

		private static string NormalizePath(string path)
		{
			return path.Replace('\\', '/');
		}

		/// <inheritdoc />
		public IFile GetEntry(string name)
		{
			return Entries.FirstOrDefault(x => x.FullName == name);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Disk: {Root}";
		}

		/// <inheritdoc />
		public void Dispose()
		{
			
		}

		public class FileSystemEntry : IFile
		{
			/// <inheritdoc />
			public string FullName { get; }

			/// <inheritdoc />
			public string Name => _fileInfo.Name;

			/// <inheritdoc />
			public long Length => _fileInfo.Length;

			private FileInfo _fileInfo;
			public FileSystemEntry(FileInfo fileInfo, string relativePath)
			{
				_fileInfo = fileInfo;
				FullName = NormalizePath(relativePath);
			}

			/// <inheritdoc />
			public Stream Open()
			{
				return _fileInfo.Open(FileMode.Open);
			}
		}
	}
}