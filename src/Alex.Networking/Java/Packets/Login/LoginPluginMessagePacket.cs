using System;
using System.Collections.Generic;
using System.Text;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Login
{
    public class LoginPluginMessagePacket : Packet<LoginPluginMessagePacket>
    {
	    public LoginPluginMessagePacket()
	    {

	    }

	    public override void Decode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }
    }
}
