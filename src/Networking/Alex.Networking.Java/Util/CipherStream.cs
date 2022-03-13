using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;

namespace Alex.Networking.Java.Util
{
	public class CipherStream : Stream
	{
		internal Stream Stream;

		public IBufferedCipher ReadCipher { get; }
		public IBufferedCipher WriteCipher { get; }

		private byte[] _mInBuf;
		private int _mInPos;
		private bool _inStreamEnded;
		public override bool CanRead => Stream.CanRead && (ReadCipher != null);

		public override bool CanWrite => Stream.CanWrite && (WriteCipher != null);


		public CipherStream(Stream stream, IBufferedCipher readCipher, IBufferedCipher writeCipher)
		{
			this.Stream = stream;

			if (readCipher != null)
			{
				this.ReadCipher = readCipher;
				_mInBuf = null;
			}

			if (writeCipher != null)
			{
				this.WriteCipher = writeCipher;
			}
		}

		#region Synchronous

		public override int ReadByte()
		{
			if (ReadCipher == null)
				return Stream.ReadByte();

			if (_mInBuf == null || _mInPos >= _mInBuf.Length)
			{
				if (!FillInBuf())
					return -1;
			}

			return _mInBuf[_mInPos++];
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (ReadCipher == null)
				return Stream.Read(buffer, offset, count);

			int num = 0;

			while (num < count)
			{
				if (_mInBuf == null || _mInPos >= _mInBuf.Length)
				{
					if (!FillInBuf())
						break;
				}

				int numToCopy = System.Math.Min(count - num, _mInBuf.Length - _mInPos);
				Array.Copy(_mInBuf, _mInPos, buffer, offset + num, numToCopy);
				_mInPos += numToCopy;
				num += numToCopy;
			}

			return num;
		}


		private bool FillInBuf()
		{
			if (_inStreamEnded)
				return false;

			_mInPos = 0;

			do
			{
				_mInBuf = ReadAndProcessBlock();
			} while (!_inStreamEnded && _mInBuf == null);

			return _mInBuf != null;
		}

		private byte[] ReadAndProcessBlock()
		{
			int blockSize = ReadCipher.GetBlockSize();
			int readSize = (blockSize == 0) ? 256 : blockSize;

			byte[] block = new byte[readSize];
			int numRead = 0;

			do
			{
				int count = Stream.Read(block, numRead, block.Length - numRead);

				if (count < 1)
				{
					_inStreamEnded = true;

					break;
				}

				numRead += count;
			} while (numRead < block.Length);

			Debug.Assert(_inStreamEnded || numRead == block.Length);

			byte[] bytes = _inStreamEnded ? ReadCipher.DoFinal(block, 0, numRead) : ReadCipher.ProcessBytes(block);

			if (bytes != null && bytes.Length == 0)
			{
				bytes = null;
			}

			return bytes;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Debug.Assert(buffer != null);
			Debug.Assert(0 <= offset && offset <= buffer.Length);
			Debug.Assert(count >= 0);

			int end = offset + count;

			Debug.Assert(0 <= end && end <= buffer.Length);

			if (WriteCipher == null)
			{
				Stream.Write(buffer, offset, count);

				return;
			}

			byte[] data = WriteCipher.ProcessBytes(buffer, offset, count);

			if (data != null)
			{
				Stream.Write(data, 0, data.Length);
			}
		}

		public override void WriteByte(byte b)
		{
			if (WriteCipher == null)
			{
				Stream.WriteByte(b);

				return;
			}

			byte[] data = WriteCipher.ProcessByte(b);

			if (data != null)
			{
				Stream.Write(data, 0, data.Length);
			}
		}

		/// <inheritdoc />
		public override void Close()
		{
			var stream = Stream;
			var cipher = WriteCipher;

			if (stream != null && cipher != null)
			{
				byte[] data = cipher.DoFinal();
				stream.Write(data, 0, data.Length);
				stream.Flush();
			}

			base.Close();
		}

		private bool _disposed = false;

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				Stream?.Dispose();
				Stream = null;
			}

			base.Dispose(disposing);
		}

		// Note: WriteCipher.DoFinal is only called during Close()
		public override void Flush() => Stream.Flush();

		#endregion

		#region Asynchronous

		private async Task<bool> FillInBufAsync()
		{
			if (_inStreamEnded)
				return false;

			_mInPos = 0;

			do
			{
				_mInBuf = await ReadAndProcessBlockAsync();
			} while (!_inStreamEnded && _mInBuf == null);

			return _mInBuf != null;
		}

		private async Task<byte[]> ReadAndProcessBlockAsync()
		{
			int blockSize = ReadCipher.GetBlockSize();
			int readSize = (blockSize == 0) ? 256 : blockSize;

			byte[] block = new byte[readSize];
			int numRead = 0;

			do
			{
				int count = await Stream.ReadAsync(block, numRead, block.Length - numRead);

				if (count < 1)
				{
					_inStreamEnded = true;

					break;
				}

				numRead += count;
			} while (numRead < block.Length);

			Debug.Assert(_inStreamEnded || numRead == block.Length);

			byte[] bytes = _inStreamEnded ? ReadCipher.DoFinal(block, 0, numRead) : ReadCipher.ProcessBytes(block);

			if (bytes != null && bytes.Length == 0)
			{
				bytes = null;
			}

			return bytes;
		}

		public override Task FlushAsync(CancellationToken cancellationToken) => Stream.FlushAsync();

		public override async ValueTask DisposeAsync()
		{
			if (WriteCipher != null)
			{
				byte[] data = WriteCipher.DoFinal();
				await Stream.WriteAsync(data, 0, data.Length);
				await Stream.FlushAsync();
			}

			await Stream.DisposeAsync();
			await base.DisposeAsync();
		}

		public override async Task<int> ReadAsync(byte[] buffer,
			int offset,
			int count,
			CancellationToken cancellationToken)
		{
			if (ReadCipher == null)
				return await Stream.ReadAsync(buffer, offset, count);

			int num = 0;

			while (num < count)
			{
				if (_mInBuf == null || _mInPos >= _mInBuf.Length)
				{
					if (!await FillInBufAsync())
						break;
				}

				if (cancellationToken.IsCancellationRequested)
					break;

				int numToCopy = Math.Min(count - num, _mInBuf.Length - _mInPos);
				Array.Copy(_mInBuf, _mInPos, buffer, offset + num, numToCopy);
				_mInPos += numToCopy;
				num += numToCopy;
			}

			return num;
		}


		public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			Debug.Assert(buffer != null);
			Debug.Assert(0 <= offset && offset <= buffer.Length);
			Debug.Assert(count >= 0);

			int end = offset + count;

			Debug.Assert(0 <= end && end <= buffer.Length);

			if (WriteCipher == null)
			{
				await Stream.WriteAsync(buffer, offset, count);

				return;
			}

			byte[] data = WriteCipher.ProcessBytes(buffer, offset, count);

			if (data != null)
			{
				await Stream.WriteAsync(data, 0, data.Length);
			}
		}

		#endregion

		#region Unimplemented & Unsupported

		public override bool CanSeek => false;

		public sealed override long Length => throw new NotSupportedException();

		public sealed override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public sealed override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		public sealed override void SetLength(long length) => throw new NotSupportedException();

		#endregion
	}
}