using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class LoginStartPacket : Packet<LoginStartPacket>
	{
		public string Username;

		public LoginStartPacket()
		{
			PacketId = 0x00;
		}

		public override void Decode(MinecraftStream stream)
		{
			Username = stream.ReadString();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteString(Username);
		}
	}
}