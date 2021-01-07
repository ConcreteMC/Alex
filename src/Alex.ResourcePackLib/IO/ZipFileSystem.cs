using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Alex.ResourcePackLib.IO.Abstract;

namespace Alex.ResourcePackLib.IO
{
	public class ZipFileSystem : IFilesystem
	{
		private  ZipArchive                       _archive;
		private  ReadOnlyCollection<ZipFileEntry> _entries;
		internal object                           Lock    = new object();
		internal Thread                           ActiveThread = null;
		public ZipFileSystem(Stream stream, string name)
		{
			Name = name;
			_archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
			
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
		public bool CanReadAsync => false;

		/// <inheritdoc />
		public IFile GetEntry(string name)
		{
			return _entries.FirstOrDefault(x => x.Name == name);
			
			var result = _archive.GetEntry(name);

			if (result != null)
				return new ZipFileEntry(this, result);

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
			private ZipFileSystem   _archive;
			public ZipFileEntry(ZipFileSystem archive, ZipArchiveEntry entry)
			{
				_archive = archive;
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
				lock (_archive.Lock)
				{
					SpinWait.SpinUntil(() => _archive.ActiveThread == null);
					_archive.ActiveThread = Thread.CurrentThread;
					
					return new StreamWrapper(_entry.Open(), () =>
					{
						_archive.ActiveThread = null;
					});
				}
			}
		}

		public class StreamWrapper : Stream
		{
			private Stream _base;
			private Action _disposeAction;
			public StreamWrapper(Stream baseStream, Action onDispose)
			{
				_base = baseStream;
				_disposeAction = onDispose;
			}

			/// <inheritdoc />
			public override void Flush()
			{
				_base.Flush();
			}

			/// <inheritdoc />
			public override int Read(byte[] buffer, int offset, int count)
			{
				return _base.Read(buffer, offset, count);
			}

			/// <inheritdoc />
			public override long Seek(long offset, SeekOrigin origin)
			{
				return _base.Seek(offset, origin);
			}

			/// <inheritdoc />
			public override void SetLength(long value)
			{
				_base.SetLength(value);
			}

			/// <inheritdoc />
			public override void Write(byte[] buffer, int offset, int count)
			{
				_base.Write(buffer, offset, count);
			}

			/// <inheritdoc />
			public override bool CanRead => _base.CanRead;

			/// <inheritdoc />
			public override bool CanSeek => _base.CanSeek;

			/// <inheritdoc />
			public override bool CanWrite => _base.CanWrite;

			/// <inheritdoc />
			public override long Length => _base.Length;

			/// <inheritdoc />
			public override long Position
			{
				get
				{
					return _base.Position;
				}
				set
				{
					_base.Position = value;
				}
			}

			/// <inheritdoc />
			protected override void Dispose(bool disposing)
			{
				_disposeAction?.Invoke();
				_disposeAction = null;
				base.Dispose(disposing);
			}
		}
	}
}