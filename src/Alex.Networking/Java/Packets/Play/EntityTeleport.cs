using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class EntityTeleport : Packet<EntityTeleport>
    {
	    public int EntityID;
	    public double X, Y, Z;
	    public byte Yaw, Pitch;
	    public bool OnGround;

	    public override void Decode(MinecraftStream stream)
	    {
		    EntityID = stream.ReadVarInt();
		    X = stream.ReadDouble();
		    Y = stream.ReadDouble();
		    Z = stream.ReadDouble();
		    Yaw = (byte) stream.ReadByte();
		    Pitch = (byte) stream.ReadByte();
		    OnGround = stream.ReadBool();
	    }

	    public override void Encode(MinecraftStream stream)
	    {
		    
	    }
    }
}
