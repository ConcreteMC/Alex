using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityVelocity : Packet<EntityVelocity>
	{
		public EntityVelocity()
		{
			PacketId = 0x40;
		}

		public int EntityId { get; set; }
		public short VelocityX, VelocityY, VelocityZ;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			VelocityX = stream.ReadShort();
			VelocityY = stream.ReadShort();
			VelocityZ = stream.ReadShort();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteShort(VelocityX);
			stream.WriteShort(VelocityY);
			stream.WriteShort(VelocityZ);
		}
	}
}