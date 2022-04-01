using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityActionPacket : Packet<EntityActionPacket>
	{
		public EntityActionPacket()
		{
			PacketId = 0x1B;
		}

		public int EntityId;
		public EntityAction Action;
		public int JumpBoost = 0;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Action = (EntityAction)stream.ReadVarInt();
			JumpBoost = stream.ReadVarInt();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteVarInt(EntityId);
			stream.WriteVarInt((int)Action);
			stream.WriteVarInt(JumpBoost);
		}
		
		public enum EntityAction
		{
			StartSneaking = 0,
			StopSneaking = 1,
			LeaveBed = 2,
			StartSprinting = 3,
			StopSprinting = 4,
			StartHorseJump = 5,
			StopHorseJump = 6,
			OpenHorseInventory = 7,
			StartElytraFlying = 8,
			Jump,

			StartFlying = 999,
			StopFlying = 998,

			StartSwimming = 997,
			StopSwimming = 996
		}
	}
}