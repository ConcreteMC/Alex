using System;
using System.Threading.Tasks;
using Alex.Interfaces;
using Alex.Networking.Java.Models;
using fNbt;

namespace Alex.Networking.Java.Util
{
	public interface IAsyncMinecraftStream
	{
		Task<byte[]> ReadAsync(int length);

		Task WriteAsync(byte[] data);

		Task<sbyte> ReadByteAsync();

		Task WriteByteAsync(byte value);

		Task<byte> ReadUnsignedByteAsync();

		Task<int> ReadIntAsync();

		Task<float> ReadFloatAsync();

		Task<bool> ReadBoolAsync();

		Task<double> ReadDoubleAsync();

		Task<int> ReadVarIntAsync();

		Task<long> ReadVarLongAsync();

		Task<short> ReadShortAsync();

		Task<ushort> ReadUShortAsync();

		Task<string> ReadStringAsync();

		Task<long> ReadLongAsync();

		Task<ulong> ReadULongAsync();

		Task<IVector3> ReadPositionAsync();

		Task<IVector3I> ReadBlockCoordinatesAsync();

		Task<SlotData> ReadSlotAsync();

		Task WriteSlotAsync(SlotData slot);

		Task WritePositionAsync(NetworkVector3 position);

		Task WritePositionAsync(IVector3I pos);

		Task<int> WriteRawVarInt32Async(uint value);

		Task<int> WriteVarIntAsync(int value);

		Task<int> WriteVarLongAsync(long value);

		Task WriteIntAsync(int data);

		Task WriteStringAsync(string data);

		Task WriteShortAsync(short data);

		Task WriteUShortAsync(ushort data);

		Task WriteBoolAsync(bool data);

		Task WriteDoubleAsync(double data);

		Task WriteFloatAsync(float data);

		Task WriteLongAsync(long data);

		Task WriteULongAsync(ulong data);

		Task WriteUuidAsync(Guid uuid);

		Task<Guid> ReadUuidAsync();

		Task<NbtCompound> ReadNbtCompoundAsync();

		Task WriteNbtCompoundAsync(NbtCompound compound);

		Task<string> ReadChatObjectAsync();
	}

	public interface IMinecraftStream
	{
		byte[] Read(int length);

		void Write(byte[] data);

		sbyte ReadSignedByte();

		byte ReadUnsignedByte();

		int ReadInt();

		float ReadFloat();

		bool ReadBool();

		double ReadDouble();

		int ReadVarInt();

		int ReadVarInt(out int bytesRead);

		long ReadVarLong();

		short ReadShort();

		ushort ReadUShort();

		string ReadString();

		long ReadLong();

		ulong ReadULong();

		IVector3 ReadPosition();

		IVector3I ReadBlockCoordinates();

		SlotData ReadSlot();

		void WriteSlot(SlotData slot);

		void WritePosition(IVector3 position);

		void WritePosition(IVector3I pos);

		int WriteRawVarInt32(uint value);

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

		void WriteULong(ulong data);

		void WriteUuid(Guid uuid);

		Guid ReadUuid();

		NbtCompound ReadNbtCompound();

		void WriteNbtCompound(NbtCompound compound);

		string ReadChatObject();
	}
}