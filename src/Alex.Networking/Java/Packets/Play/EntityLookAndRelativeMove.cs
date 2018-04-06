using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityLookAndRelativeMove : Packet<EntityLookAndRelativeMove>
	{
		public EntityLookAndRelativeMove()
		{
			PacketId = 0x28;
		}

		public int EntityId { get; set; }
		public short DeltaX, DeltaY, DeltaZ;
		public byte Yaw, Pitch;
		public bool OnGround;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			DeltaX = stream.ReadShort();
			DeltaY = stream.ReadShort();
			DeltaZ = stream.ReadShort();
			Yaw = (byte) stream.ReadByte();
			Pitch = (byte) stream.ReadByte();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteShort(DeltaX);
			stream.WriteShort(DeltaY);
			stream.WriteShort(DeltaZ);
			stream.WriteByte(Yaw);
			stream.WriteByte(Pitch);
			stream.WriteBool(OnGround);
		}
	}
}