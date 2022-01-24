using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Handshake
{
	public class HandshakePacket : Packet<HandshakePacket>
	{
		public int ProtocolVersion;
		public string ServerAddress;
		public ushort ServerPort;
		public ConnectionState NextState;

		public HandshakePacket()
		{
			PacketId = 0x00;
		}

		public override void Decode(MinecraftStream stream)
		{
			ProtocolVersion = stream.ReadVarInt();
			ServerAddress = stream.ReadString();
			ServerPort = stream.ReadUShort();
			NextState = (ConnectionState)stream.ReadVarInt();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(ProtocolVersion);
			stream.WriteString(ServerAddress);
			stream.WriteUShort(ServerPort);
			stream.WriteVarInt((int)NextState);
		}
	}
}