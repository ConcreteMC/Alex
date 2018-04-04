using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class HeldItemChangePacket : Packet<HeldItemChangePacket>
    {
	    public HeldItemChangePacket()
	    {
		    PacketId = 0x3C;
	    }

	    public byte Slot;

	    public override void Decode(MinecraftStream stream)
	    {
		    Slot = (byte)stream.ReadByte();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    stream.WriteByte(Slot);
	    }
    }
}
