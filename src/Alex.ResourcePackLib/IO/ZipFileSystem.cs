using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Alex.ResourcePackLib.IO.Abstract;

namespace Alex.ResourcePackLib.IO
{
	public class ZipFileSystem : IFilesystem
	{
		private          ZipArchive                       _archive;
		private          ReadOnlyCollection<ZipFileEntry> _entries;
		public ZipFileSystem(Stream stream, string name)
		{
			Name = name;
			_archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
			
			List<ZipFileEntry> entries = new List<ZipFileEntry>();
			foreach (var entry in _archive.Entries)
			{
				entries.Add(new ZipFileEntry(entry));
			}

			_entries = new ReadOnlyCollection<ZipFileEntry>(entries);
		}

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public IReadOnlyCollection<IFile> Entries 
		{
			get
			{
				return _entries;
			} 
		}

		/// <inheritdoc />
		public bool CanReadAsync => false;

		/// <inheritdoc />
		public IFile GetEntry(string name)
		{
			var result = _archive.GetEntry(name);

			if (result != null)
				return new ZipFileEntry(result);

			return null;
		}
		
		public override string ToString()
		{
			return $"Zip: {Name}";
		}
		
		/// <inheritdoc />
		public void Dispose()
		{
			_archive.Dispose();
		}

		public class ZipFileEntry : IFile
		{
			private ZipArchiveEntry _entry;
			public ZipFileEntry(ZipArchiveEntry entry)
			{
				_entry = entry;
			}

			/// <inheritdoc />
			public string FullName => _entry.FullName;

			/// <inheritdoc />
			public string Name => _entry.Name;

			/// <inheritdoc />
			public long Length => _entry.Length;

			/// <inheritdoc />
			public Stream Open()
			{
				return _entry.Open();
			}
		}
	}
}