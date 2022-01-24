using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PlayerLookPacket : Packet<PlayerLookPacket>
	{
		public float Yaw;
		public float Pitch;
		public bool OnGround;

		public PlayerLookPacket()
		{
			PacketId = 0x13;
		}

		public override void Decode(MinecraftStream stream)
		{
			Yaw = stream.ReadFloat();
			Pitch = stream.ReadFloat();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteFloat(Yaw);
			stream.WriteFloat(Pitch);
			stream.WriteBool(OnGround);
		}
	}
}