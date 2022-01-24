using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class OpenWindowPacket : Packet<OpenWindowPacket>
	{
		public int WindowId { get; set; }
		public int WindowType { get; set; }
		public string WindowTitle { get; set; }

		public override void Decode(MinecraftStream stream)
		{
			WindowId = stream.ReadVarInt();
			WindowType = stream.ReadVarInt();
			WindowTitle = stream.ReadString();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}
	}
}