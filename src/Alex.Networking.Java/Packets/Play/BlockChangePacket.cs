using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class BlockChangePacket : Packet<BlockChangePacket>
    {

	    public BlockChangePacket()
	    {
		    PacketId = 0x0B;
	    }

	    public BlockCoordinates Location;
	    public uint PalleteId;

	    public override void Decode(MinecraftStream stream)
	    {
		    Location = stream.ReadPosition();
		    PalleteId = (uint) stream.ReadVarInt();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    
	    }
    }
}
