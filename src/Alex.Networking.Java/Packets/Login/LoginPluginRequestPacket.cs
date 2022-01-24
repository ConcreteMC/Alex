using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class LoginPluginRequestPacket : Packet<LoginPluginRequestPacket>
	{
		public int MessageId;
		public string Channel;
		public byte[] Data;

		public LoginPluginRequestPacket()
		{
			PacketId = 0x04;
		}

		public override void Decode(MinecraftStream stream)
		{
			MessageId = stream.ReadVarInt();
			Channel = stream.ReadString();

			long length = stream.Length - stream.Position;
			byte[] buffer = new byte[length];
			Data = stream.Read(buffer.Length);
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(MessageId);
			stream.WriteString(Channel);
			stream.Write(Data);
		}
	}
}