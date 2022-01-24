using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class ServerDifficultyPacket : Packet<ServerDifficultyPacket>
	{
		public ServerDifficultyPacket()
		{
			PacketId = 0x0D;
		}

		public byte Difficulty;

		public override void Decode(MinecraftStream stream)
		{
			Difficulty = (byte)stream.ReadByte();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteByte(Difficulty);
		}
	}
}