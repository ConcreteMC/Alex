using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class LoginPluginResponsePacket : Packet<LoginPluginResponsePacket>
	{
		public int MessageId;
		public bool Succesful;
		public byte[] Data;

		public LoginPluginResponsePacket()
		{
			PacketId = 0x02;
		}

		public override void Decode(MinecraftStream stream) { }

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(MessageId);
			stream.WriteBool(Succesful);
			stream.Write(Data);
		}
	}
}