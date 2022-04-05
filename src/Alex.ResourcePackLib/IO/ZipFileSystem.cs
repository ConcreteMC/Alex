using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Alex.ResourcePackLib.IO.Abstract;

namespace Alex.ResourcePackLib.IO
{
	public class ZipFileSystem : IFilesystem
	{
		private ZipArchive _archive;
		private ReadOnlyCollection<ZipFileEntry> _entries;
		internal object Lock = new object();
		internal Thread ActiveThread = null;

		public ZipFileSystem(Stream stream, string name)
		{
			Name = name;
			_archive = new ZipArchive(stream);

			List<ZipFileEntry> entries = new List<ZipFileEntry>();

			foreach (var entry in _archive.Entries)
			{
				entries.Add(new ZipFileEntry(this, entry));
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
		public IFile GetEntry(string name)
		{
			name = DiskFileSystem.NormalizePath(name);
			return _entries.FirstOrDefault(x => x.FullName == name);
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
			private ZipFileSystem _archive;

			public ZipFileEntry(ZipFileSystem archive, ZipArchiveEntry entry)
			{
				_archive = archive;
				_entry = entry;

				FullName = DiskFileSystem.NormalizePath(entry.FullName);
				Name = Path.GetFileName(FullName);
				Length = entry.Length;
			}

			/// <inheritdoc />
			public string FullName { get; }

			/// <inheritdoc />
			public string Name { get; }

			/// <inheritdoc />
			public long Length { get; }

			/// <inheritdoc />
			public Stream Open()
			{
				lock (_archive.Lock)
				{
					SpinWait.SpinUntil(() => _archive.ActiveThread == null);
					_archive.ActiveThread = Thread.CurrentThread;

					byte[] buffer;

					using (MemoryStream ms = new MemoryStream())
					{
						using (var s = _entry.Open())
							s.CopyTo(ms);

						buffer = ms.ToArray();
					}

					return new StreamWrapper(new MemoryStream(buffer), () => { _archive.ActiveThread = null; });
				}
			}
		}
	}
}