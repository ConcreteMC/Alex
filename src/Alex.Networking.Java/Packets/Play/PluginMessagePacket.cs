using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PluginMessagePacket : Packet<PluginMessagePacket>
	{
		public PluginMessagePacket()
		{
			PacketId = 0x0B;
		}

		public string Channel;
		public byte[] Data;

		public override void Decode(MinecraftStream stream)
		{
			Channel = stream.ReadString();
			var l = stream.ReadVarInt();
			Data = stream.Read(l);
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteString(Channel);
			stream.WriteVarInt(Data.Length);
			stream.Write(Data, 0, Data.Length);
		}
	}
}