using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityAnimationPacket : Packet<EntityAnimationPacket>
	{
		public EntityAnimationPacket() { }

		public int EntityId { get; set; }
		public Animations Animation { get; set; }

		/// <inheritdoc />
		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Animation = (Animations)stream.ReadByte();
		}

		/// <inheritdoc />
		public override void Encode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public enum Animations
		{
			SwingMainArm = 0,
			TakeDamage = 1,
			LeaveBed = 2,
			SwingOffhand = 3,
			CriticalEffect = 4,
			MagicCriticalEffect = 5
		}
	}
}