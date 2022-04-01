using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class ChatMessagePacket : Packet<ChatMessagePacket>
	{
		public const byte Chat = 0;
		public const byte System = 1;
		public const byte Hotbar = 2;

		public bool ServerBound = false;

		public ChatMessagePacket()
		{
			PacketId = 0x03;
		}

		public string Message;
		public byte Position = Chat;
		public Guid UUID = Guid.NewGuid();

		public override void Decode(MinecraftStream stream)
		{
			Message = stream.ReadString();

			if (!ServerBound)
			{
				Position = (byte)stream.ReadByte();
				UUID = stream.ReadUuid();
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteString(Message);

			if (!ServerBound)
			{
				stream.WriteByte(Position);
				stream.WriteUuid(UUID);
			}
		}

		/// <inheritdoc />
		protected override void ResetPacket()
		{
			base.ResetPacket();

			ServerBound = false;
			Position = Chat;
		}
	}
}