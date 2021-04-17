using System;
using Alex.API.Data;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityEquipmentPacket : Packet<EntityEquipmentPacket>
	{
		public int EntityId;
		public SlotEnum Slot;
		public SlotData Item;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();
			Slot = (SlotEnum) stream.ReadVarInt();
			Item = stream.ReadSlot();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}

		public enum SlotEnum
		{
			MainHand = 0,
			OffHand = 1,
			Boots = 2,
			Leggings = 3,
			Chestplate = 4,
			Helmet = 5
		}
	}
}
