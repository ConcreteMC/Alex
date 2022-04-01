using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play

{
	public class SpawnPlayerPacket : Packet<SpawnPlayerPacket>
	{
		public SpawnPlayerPacket()
		{
			PacketId = 0x05;
		}

		public int EntityId;
		public Guid Uuid;
		public double X;
		public double Y;
		public double Z;
		public sbyte Yaw;
		public sbyte Pitch;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Uuid = stream.ReadUuid();
			X = stream.ReadDouble();
			Y = stream.ReadDouble();
			Z = stream.ReadDouble();
			Yaw = (sbyte)stream.ReadByte();
			Pitch = (sbyte)stream.ReadByte();
			stream.ReadByte();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteUuid(Uuid);
			stream.WriteDouble(X);
			stream.WriteDouble(Y);
			stream.WriteDouble(Z);
			stream.WriteByte((byte)Yaw);
			stream.WriteByte((byte)Pitch);
			stream.WriteByte(0xff); //End of metadata
		}
	}
}