using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityHeadLook : Packet<EntityHeadLook>
	{
		public EntityHeadLook()
		{
			PacketId = 0x39;
		}

		public int EntityId { get; set; }
		public sbyte HeadYaw;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			HeadYaw = (sbyte)stream.ReadByte();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteByte((byte)HeadYaw);
		}
	}
}