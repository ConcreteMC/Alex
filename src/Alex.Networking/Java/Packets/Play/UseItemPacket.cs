using System;
using System.Collections.Generic;
using System.Text;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class UseItemPacket : Packet<UseItemPacket>
    {
	    public int Hand = 0;
	    public UseItemPacket()
	    {
		    PacketId = 0x2A;
	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    stream.WriteVarInt(Hand);
	    }
    }
}
