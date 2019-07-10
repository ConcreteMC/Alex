using Alex.API.Utils;
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
	    public int PalleteId;

	    public override void Decode(MinecraftStream stream)
	    {
		    Location = stream.ReadPosition();
		    PalleteId = stream.ReadVarInt();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    
	    }
    }
}
