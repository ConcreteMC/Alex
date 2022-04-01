using Alex.Interfaces;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class BlockChangePacket : Packet<BlockChangePacket>
	{
		public BlockChangePacket()
		{
			PacketId = 0x0B;
		}

		public IVector3I Location;
		public uint PalleteId;

		public override void Decode(MinecraftStream stream)
		{
			Location = stream.ReadBlockCoordinates();
			PalleteId = (uint)stream.ReadVarInt();
		}

		public override void Encode(MinecraftStream stream) { }
	}
}