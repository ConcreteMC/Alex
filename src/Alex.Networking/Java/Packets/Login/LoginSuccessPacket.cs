using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
	public class LoginSuccessPacket : Packet<LoginSuccessPacket>
	{
		public LoginSuccessPacket()
		{
			PacketId = 0x03;
		}

		public string UUID;
		public string Username;

		public override void Decode(MinecraftStream stream)
		{
			UUID = stream.ReadString();
			Username = stream.ReadString();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteString(UUID);
			stream.WriteString(Username);
		}
	}
}
