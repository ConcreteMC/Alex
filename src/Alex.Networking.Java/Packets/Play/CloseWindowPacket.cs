using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class CloseWindowPacket : Packet<CloseWindowPacket>
	{
		public CloseWindowPacket()
		{
			PacketId = 0x09;
		}

		public byte WindowId { get; set; }

		public override void Decode(MinecraftStream stream)
		{
			WindowId = (byte)stream.ReadByte();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteByte(WindowId);
		}
	}
}