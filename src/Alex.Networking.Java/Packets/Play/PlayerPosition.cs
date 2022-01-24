using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PlayerPosition : Packet<PlayerPosition>
	{
		public double X;
		public double FeetY;
		public double Z;
		public bool OnGround;

		public PlayerPosition()
		{
			PacketId = 0x11;
		}

		public override void Decode(MinecraftStream stream)
		{
			X = stream.ReadDouble();
			FeetY = stream.ReadDouble();
			Z = stream.ReadDouble();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteDouble(X);
			stream.WriteDouble(FeetY);
			stream.WriteDouble(Z);
			stream.WriteBool(OnGround);
		}
	}
}