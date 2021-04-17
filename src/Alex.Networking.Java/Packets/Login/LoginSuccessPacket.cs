using System;
using Alex.Networking.Java.Util;
using MiNET.Utils;

namespace Alex.Networking.Java.Packets.Login
{
	public class LoginSuccessPacket : Packet<LoginSuccessPacket>
	{
		public LoginSuccessPacket()
		{
			PacketId = 0x03;
		}

		public UUID UUID;
		public string Username;

		public override void Decode(MinecraftStream stream)
		{
			UUID = stream.ReadUuid();
			Username = stream.ReadString();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteUuid(UUID);
			stream.WriteString(Username);
		}
	}
}
