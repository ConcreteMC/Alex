using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PlayerPositionAndLookPacket : Packet<PlayerPositionAndLookPacket>
	{
		public PlayerPositionAndLookPacket()
		{
			PacketId = 0x13;
		}

		public double X;
		public double Y;
		public double Z;
		public float Yaw;
		public float Pitch;
		public byte Flags = 0;
		public int TeleportId;
		public bool DismountVehicle = false;

		public override void Decode(MinecraftStream stream)
		{
			X = stream.ReadDouble();
			Y = stream.ReadDouble();
			Z = stream.ReadDouble();
			Yaw = stream.ReadFloat();
			Pitch = stream.ReadFloat();
			Flags = (byte)stream.ReadByte();
			TeleportId = stream.ReadVarInt();
			DismountVehicle = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteDouble(X);
			stream.WriteDouble(Y);
			stream.WriteDouble(Z);
			stream.WriteFloat(Yaw);
			stream.WriteFloat(Pitch);
			stream.WriteByte(Flags);
			stream.WriteVarInt(TeleportId);
			stream.WriteBool(DismountVehicle);
		}
	}

	public class PlayerPositionAndLookPacketServerBound : Packet<PlayerPositionAndLookPacketServerBound>
	{
		public PlayerPositionAndLookPacketServerBound()
		{
			PacketId = 0x12;
		}

		public double X;
		public double Y;
		public double Z;
		public float Yaw;
		public float Pitch;
		public bool OnGround;

		public override void Decode(MinecraftStream stream)
		{
			X = stream.ReadDouble();
			Y = stream.ReadDouble();
			Z = stream.ReadDouble();
			Yaw = stream.ReadFloat();
			Pitch = stream.ReadFloat();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteDouble(X);
			stream.WriteDouble(Y);
			stream.WriteDouble(Z);
			stream.WriteFloat(Yaw);
			stream.WriteFloat(Pitch);
			stream.WriteBool(OnGround);
		}
	}
}