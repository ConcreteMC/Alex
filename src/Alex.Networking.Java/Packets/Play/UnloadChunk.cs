using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class UnloadChunk : Packet<UnloadChunk>
	{
		public UnloadChunk()
		{
			PacketId = 0x1D;
		}

		public int X;
		public int Z;

		public override void Decode(MinecraftStream stream)
		{
			X = stream.ReadInt();
			Z = stream.ReadInt();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteInt(X);
			stream.WriteInt(Z);
		}
	}
}