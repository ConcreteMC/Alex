using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class SetCompressionPacket : Packet<SetCompressionPacket>
	{
		public int Threshold;

		public SetCompressionPacket()
		{
			PacketId = 0x04;
		}

		public override void Decode(MinecraftStream stream)
		{
			Threshold = stream.ReadVarInt();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(Threshold);
		}
	}
}