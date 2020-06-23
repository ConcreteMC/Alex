using System;
using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class PlayerDiggingPacket : Packet<PlayerDiggingPacket>
    {
	    public DiggingStatus Status;
	    public BlockCoordinates Location;
	    public BlockFace Face;

	    public PlayerDiggingPacket()
	    {
		    PacketId = 0x1B;
	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    stream.WriteVarInt((int) Status);
            stream.WritePosition(Location);
		    switch (Face)
		    {
			    case BlockFace.Down:
				    stream.WriteByte(0);
				    break;
			    case BlockFace.Up:
				    stream.WriteByte(1);
				    break;
			    case BlockFace.North:
				    stream.WriteByte(2);
				    break;
			    case BlockFace.South:
				    stream.WriteByte(3);
				    break;
			    case BlockFace.West:
				    stream.WriteByte(4);
				    break;
			    case BlockFace.East:
				    stream.WriteByte(5);
				    break;
			    case BlockFace.None:
				    stream.WriteByte(1);
				    break;
		    }
        }
    }
}
