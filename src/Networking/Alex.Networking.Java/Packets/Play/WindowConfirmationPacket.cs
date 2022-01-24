using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class WindowConfirmationPacket : Packet<WindowConfirmationPacket>
	{
		public WindowConfirmationPacket()
		{
			PacketId = 0x07;
		}

		public byte WindowId { get; set; }
		public short ActionNumber { get; set; }
		public bool Accepted { get; set; }

		public override void Decode(MinecraftStream stream)
		{
			WindowId = (byte)stream.ReadByte();
			ActionNumber = stream.ReadShort();
			Accepted = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteByte(WindowId);
			stream.WriteShort(ActionNumber);
			stream.WriteBool(Accepted);
		}
	}
}