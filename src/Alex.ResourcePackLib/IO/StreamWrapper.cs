using System;
using System.IO;

namespace Alex.ResourcePackLib.IO
{
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