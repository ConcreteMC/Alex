using System;
using System.Collections.Generic;
using Alex.Networking.Java.Models;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class EntityEquipmentPacket : Packet<EntityEquipmentPacket>
	{
		public int EntityId;
		public EquipmentSlot[] Slots;

		public override void Decode(MinecraftStream stream)
		{
			EntityId = stream.ReadVarInt();

			List<EquipmentSlot> slots = new List<EquipmentSlot>();

			byte slotValue = 0;

			do
			{
				slotValue = (byte)stream.ReadByte();
				SlotEnum slotEnum = (SlotEnum)slotValue;

				var item = stream.ReadSlot();
				slots.Add(new EquipmentSlot() { Data = item, Slot = slotEnum });
			} while ((slotValue & (1 << 7)) != 0);

			Slots = slots.ToArray();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}

		public enum SlotEnum : byte
		{
			MainHand = 0,
			OffHand = 1,
			Boots = 2,
			Leggings = 3,
			Chestplate = 4,
			Helmet = 5
		}

		public class EquipmentSlot
		{
			public SlotEnum Slot { get; set; }
			public SlotData Data { get; set; }
		}
	}
}