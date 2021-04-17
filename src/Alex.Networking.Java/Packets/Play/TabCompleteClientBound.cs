using System;
using Alex.API.Data;
using Alex.API.Utils;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class TabCompleteClientBound : Packet<TabCompleteClientBound>
    {
	    public int TransactionId;
	    public int Start;
	    public int Length;

	    public TabCompleteMatch[] Matches;
	    public override void Decode(MinecraftStream stream)
	    {
		    TransactionId = stream.ReadVarInt();
		    Start = stream.ReadVarInt();
		    Length = stream.ReadVarInt();

		    int c = stream.ReadVarInt();
			Matches = new TabCompleteMatch[c];
		    for (int i = 0; i < c; i++)
		    {
				var entry = new TabCompleteMatch();
			    entry.Match = stream.ReadString();
			    entry.HasTooltip = stream.ReadBool();

			    if (entry.HasTooltip)
			    {
				    ChatObject.TryParse(stream.ReadString(), out entry.Tooltip);
			    }

			    Matches[i] = entry;
		    }
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }
    }

}
