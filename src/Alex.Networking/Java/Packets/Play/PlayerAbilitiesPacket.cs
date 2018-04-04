using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class PlayerAbilitiesPacket : Packet<PlayerAbilitiesPacket>
    {
	    public PlayerAbilitiesPacket()
	    {
		    PacketId = 0x2D;
	    }

	    public byte Flags;
	    public float FlyingSpeed;
	    public float FiedOfViewModifier;

		public override void Decode(MinecraftStream stream)
		{
			Flags = (byte)stream.ReadByte();
			FlyingSpeed = stream.ReadFloat();
			FiedOfViewModifier = stream.ReadFloat();
		}

	    public override void Encode(MinecraftStream stream)
	    {
		    throw new NotImplementedException();
	    }
    }
}
