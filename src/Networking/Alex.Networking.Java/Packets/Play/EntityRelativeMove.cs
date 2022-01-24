using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityRelativeMove : Packet<EntityRelativeMove>
	{
		public EntityRelativeMove()
		{
			PacketId = 0x27;
		}

		public int EntityId { get; set; }
		public short DeltaX, DeltaY, DeltaZ;
		public bool OnGround;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			DeltaX = stream.ReadShort();
			DeltaY = stream.ReadShort();
			DeltaZ = stream.ReadShort();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteShort(DeltaX);
			stream.WriteShort(DeltaY);
			stream.WriteShort(DeltaZ);
			stream.WriteBool(OnGround);
		}
	}
}