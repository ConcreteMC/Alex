using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using fNbt;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
	public interface IMinecraftStream
	{
		Stream BaseStream { get; }
		bool CanRead { get; }
		bool CanSeek { get; }
		bool CanWrite { get; }
		long Length { get; }
		long Position { get; set; }
		bool CanTimeout { get; }
		int ReadTimeout { get; set; }
		int WriteTimeout { get; set; }
		void InitEncryption(byte[] key, bool write);
		long Seek(long offset, SeekOrigin origin);
		void SetLength(long value);
		int Read(byte[] buffer, int offset, int count);
		void Write(byte[] buffer, int offset, int count);
		void Flush();
		int ReadByte();
		byte[] Read(int length);
		int ReadInt();
		float ReadFloat();
		bool ReadBool();
		double ReadDouble();
		int ReadVarInt();
		int ReadVarInt(out int bytesRead);
		long ReadVarLong();
		short ReadShort();
		ushort ReadUShort();
		ushort[] ReadUShort(int count);
		ushort[] ReadUShortLocal(int count);
		short[] ReadShortLocal(int count);
		string ReadString();
		long ReadLong();
		Vector3 ReadPosition();
		void Write(byte[] data);
		void WritePosition(Vector3 position);
		int WriteVarInt(int value);
		int WriteVarLong(long value);
		void WriteInt(int data);
		void WriteString(string data);
		void WriteShort(short data);
		void WriteUShort(ushort data);
		void WriteBool(bool data);
		void WriteDouble(double data);
		void WriteFloat(float data);
		void WriteLong(long data);
		void WriteUuid(Guid uuid);
		Guid ReadUuid();
		NbtCompound ReadNbtCompound();
		void WriteNbtCompound(NbtCompound compound);
		IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
		IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state);
		void Close();
		void CopyTo(Stream destination);
		void CopyTo(Stream destination, int bufferSize);
		Task CopyToAsync(Stream destination);
		Task CopyToAsync(Stream destination, int bufferSize);
		Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken);
		void Dispose();
		int EndRead(IAsyncResult asyncResult);
		void EndWrite(IAsyncResult asyncResult);
		Task FlushAsync();
		Task FlushAsync(CancellationToken cancellationToken);
		Task<int> ReadAsync(byte[] buffer, int offset, int count);
		Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
		Task WriteAsync(byte[] buffer, int offset, int count);
		Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
		void WriteByte(byte value);
		object GetLifetimeService();
		object InitializeLifetimeService();
	}
}
