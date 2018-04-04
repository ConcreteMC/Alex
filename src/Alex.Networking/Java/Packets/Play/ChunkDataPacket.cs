using System.Collections.Generic;
using System.IO;
using Alex.Networking.Java.Util;
using fNbt;
using fNbt.Tags;

namespace Alex.Networking.Java.Packets.Play
{
	public class ChunkDataPacket : Packet<ChunkDataPacket>
	{
		public int ChunkX;
		public int ChunkZ;
		public int AvailableSections;
		public byte[] Buffer;
		public List<NbtCompound> TileEntities;
		public bool FullChunk;

		public ChunkDataPacket()
		{
			PacketId = 0x21;
			TileEntities = new List<NbtCompound>();
		}

	
		public override void Decode(MinecraftStream stream)
		{
			ChunkX = stream.ReadInt();
			ChunkZ = stream.ReadInt();
			FullChunk = stream.ReadBool();
			AvailableSections = stream.ReadVarInt();

			int i = stream.ReadVarInt();
			Buffer = new byte[i];
			stream.Read(Buffer, 0, Buffer.Length);

			//int tileEntities = stream.ReadVarInt();
			//for (int k = 0; k < tileEntities; k++)
			//{
			//	TileEntities.Add(stream.ReadNbtCompound());
			//}
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteInt(ChunkX);
			stream.WriteInt(ChunkZ);
			stream.WriteBool(FullChunk);
			stream.WriteVarInt(AvailableSections);
			stream.WriteVarInt(Buffer.Length);
			stream.Write(Buffer, 0, Buffer.Length);
			//stream.WriteVarInt(TileEntities.Count);
			//foreach (var tileEntity in TileEntities)
			//{
			//	stream.WriteNbtCompound(tileEntity);
			//}
		}
	}
}
