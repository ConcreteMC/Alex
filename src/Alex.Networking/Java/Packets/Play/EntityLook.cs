using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityLook : Packet<EntityLook>
	{
		public EntityLook()
		{
			PacketId = 0x29;
		}

		public int EntityId { get; set; }
		public byte Yaw, Pitch;
		public bool OnGround;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Yaw = (byte)stream.ReadByte();
			Pitch = (byte)stream.ReadByte();
			OnGround = stream.ReadBool();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteByte(Yaw);
			stream.WriteByte(Pitch);
			stream.WriteBool(OnGround);
		}
	}
}