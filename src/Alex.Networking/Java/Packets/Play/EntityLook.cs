using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityLook : Packet<EntityLook>
	{
		public EntityLook()
		{
			PacketId = 0x2A;
		}

		public int EntityId { get; set; }
		public sbyte Yaw, Pitch;
		public bool OnGround;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Yaw = (sbyte)stream.ReadByte();
			Pitch = (sbyte)stream.ReadByte();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteByte((byte)Yaw);
			stream.WriteByte((byte)Pitch);
			stream.WriteBool(OnGround);
		}
	}
}