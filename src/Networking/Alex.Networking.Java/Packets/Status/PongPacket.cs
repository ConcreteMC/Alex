using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Status
{
	public class PongPacket : Packet<PongPacket>
	{
		public PongPacket()
		{
			PacketId = 0x01;
		}

		public long PingId { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			PingId = stream.ReadLong();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			stream.WriteLong(PingId);
		}
	}
}