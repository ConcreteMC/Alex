using System.Collections.Generic;
using Alex.Networking.Java.Util;
using fNbt;

namespace Alex.Networking.Java.Packets.Play
{
	public class ChunkDataPacket : Packet<ChunkDataPacket>
	{
		public int ChunkX;
		public int ChunkZ;
		public int PrimaryBitmask;
		public byte[] Buffer;
		public List<NbtCompound> TileEntities;
		public NbtCompound HeightMaps;
		public bool GroundUp;

		public ChunkDataPacket()
		{
			PacketId = 0x22;
			TileEntities = new List<NbtCompound>();
			//HeightMaps = new NbtCompound();
		}

	
		public override void Decode(MinecraftStream stream)
		{
			ChunkX = stream.ReadInt();
			ChunkZ = stream.ReadInt();
			GroundUp = stream.ReadBool();
			PrimaryBitmask = stream.ReadVarInt();

			HeightMaps = stream.ReadNbtCompound();

			int i = stream.ReadVarInt();
			Buffer = new byte[i];
			stream.Read(Buffer, 0, Buffer.Length);

			int tileEntities = stream.ReadVarInt();
			for (int k = 0; k < tileEntities; k++)
			{
				TileEntities.Add(stream.ReadNbtCompound());
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteInt(ChunkX);
			stream.WriteInt(ChunkZ);
			stream.WriteBool(GroundUp);
			stream.WriteVarInt(PrimaryBitmask);
			stream.WriteVarInt(Buffer.Length);
			stream.Write(Buffer, 0, Buffer.Length);
			stream.WriteVarInt(TileEntities.Count);
			foreach (var tileEntity in TileEntities)
			{
				stream.WriteNbtCompound(tileEntity);
			}
		}
	}
}
