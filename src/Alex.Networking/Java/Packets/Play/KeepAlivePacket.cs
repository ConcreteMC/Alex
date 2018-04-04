using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class KeepAlivePacket : Packet<KeepAlivePacket>
	{
		public KeepAlivePacket()
		{
			PacketId = 0x1F; //Clientbound
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
