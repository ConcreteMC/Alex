using System;
using System.Collections.Generic;
using System.Text;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class TabCompleteServerBound : Packet<TabCompleteServerBound>
    {
	    public int TransactionId;
	    public string Text;

	    public TabCompleteServerBound()
	    {
		    PacketId = 0x05;
	    }

		public override void Decode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    stream.WriteVarInt(TransactionId);
			stream.WriteString(Text);
	    }
    }
}
