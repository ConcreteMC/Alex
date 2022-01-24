using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityLookAndRelativeMove : Packet<EntityLookAndRelativeMove>
	{
		public EntityLookAndRelativeMove()
		{
			PacketId = 0x2A;
		}

		public int EntityId { get; set; }
		public short DeltaX, DeltaY, DeltaZ;
		public sbyte Yaw, Pitch;
		public bool OnGround;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			DeltaX = stream.ReadShort();
			DeltaY = stream.ReadShort();
			DeltaZ = stream.ReadShort();
			Yaw = (sbyte)stream.ReadByte();
			Pitch = (sbyte)stream.ReadByte();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteShort(DeltaX);
			stream.WriteShort(DeltaY);
			stream.WriteShort(DeltaZ);
			stream.WriteByte((byte)Yaw);
			stream.WriteByte((byte)Pitch);
			stream.WriteBool(OnGround);
		}
	}
}