using System.Threading.Tasks;
using Alex.Common.Data;
using Alex.Common.Utils.Vectors;
using fNbt;
using Microsoft.Xna.Framework;

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

		Task<Vector3> ReadPositionAsync();

		Task<BlockCoordinates> ReadBlockCoordinatesAsync();

		Task<SlotData> ReadSlotAsync();

		Task WriteSlotAsync(SlotData slot);

		Task WritePositionAsync(Vector3 position);

		Task WritePositionAsync(BlockCoordinates pos);

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

		Task WriteUuidAsync(MiNET.Utils.UUID uuid);

		Task<MiNET.Utils.UUID> ReadUuidAsync();

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

		Vector3 ReadPosition();

		BlockCoordinates ReadBlockCoordinates();

		SlotData ReadSlot();

		void WriteSlot(SlotData slot);

		void WritePosition(Vector3 position);

		void WritePosition(BlockCoordinates pos);

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

		void WriteUuid(MiNET.Utils.UUID uuid);

		MiNET.Utils.UUID ReadUuid();

		NbtCompound ReadNbtCompound();

		void WriteNbtCompound(NbtCompound compound);

		string ReadChatObject();
	}
}