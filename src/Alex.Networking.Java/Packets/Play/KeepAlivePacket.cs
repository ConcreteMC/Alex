using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class KeepAlivePacket : Packet<KeepAlivePacket>
	{
		public KeepAlivePacket()
		{
			PacketId = 0x10; //Clientbound
		}

		public long KeepAliveid;

		public override void Decode(MinecraftStream stream)
		{
			KeepAliveid = stream.ReadLong();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteLong(KeepAliveid);
		}
	}
}
