using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SpawnLivingEntity : Packet<SpawnLivingEntity>
	{
		public int EntityId;
		public Guid Uuid;
		public int Type;
		public double X;
		public double Y;
		public double Z;
		public byte Yaw;
		public byte Pitch;
		public byte HeadPitch;
		public short VelocityX;
		public short VelocityY;
		public short VelocityZ;

		public SpawnLivingEntity()
		{
			PacketId = 0x03;
		}

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Uuid = stream.ReadUuid();
			Type = stream.ReadVarInt();
			X = stream.ReadDouble();
			Y = stream.ReadDouble();
			Z = stream.ReadDouble();
			Yaw = (byte)stream.ReadByte();
			Pitch = (byte)stream.ReadByte();
			HeadPitch = (byte)stream.ReadByte();
			VelocityX = stream.ReadShort();
			VelocityY = stream.ReadShort();
			VelocityZ = stream.ReadShort();
		}

		public override void Encode(MinecraftStream stream) { }
	}
}