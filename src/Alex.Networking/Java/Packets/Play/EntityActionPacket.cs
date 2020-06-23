using Alex.API.Utils;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
    public class EntityActionPacket : Packet<EntityActionPacket>
    {
	    public EntityActionPacket()
	    {
		    PacketId = 0x1C;
		    
		    Category = PacketCategory.EntityMovement;
	    }

	    public int EntityId;
	    public EntityAction Action;
	    public int JumpBoost = 0;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Action = (EntityAction) stream.ReadVarInt();
			JumpBoost = stream.ReadVarInt();
		}

	    public override void Encode(MinecraftStream stream)
	    {
		    stream.WriteVarInt(EntityId);
		    stream.WriteVarInt((int) Action);
		    stream.WriteVarInt(JumpBoost);
	    }
    }
}
