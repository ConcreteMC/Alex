using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class PongPacket : Packet<PongPacket>
	{
		public PongPacket()
		{
			PacketId = 0x1D;
		}
		public int PingId { get; set; }
		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			PingId = stream.ReadInt();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			stream.WriteInt(PingId);
		}
	}
}