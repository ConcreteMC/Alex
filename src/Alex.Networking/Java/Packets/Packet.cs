using Alex.Networking.Java.Framework;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets
{
	public abstract class Packet : IPacket<MinecraftStream>
	{
		public int PacketId { get; set; } = -1;

		public abstract void Decode(MinecraftStream stream);

		public abstract void Encode(MinecraftStream stream);
	}

	public abstract class Packet<TPacket> : Packet where TPacket : Packet<TPacket>, new()
	{
		public static TPacket CreateObject()
		{
			return new TPacket();
		}
	}
}
