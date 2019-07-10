using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class EntityMetadataPacket : Packet<EntityMetadataPacket>
    {
	    public int EntityId;
		//public 
	    public EntityMetadataPacket()
	    {

	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    EntityId = stream.ReadVarInt();
			//TODO: Read metadata properly
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }
    }
}
