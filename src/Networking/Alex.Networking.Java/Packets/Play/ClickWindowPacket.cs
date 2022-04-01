using Alex.Networking.Java.Models;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class ClickWindowPacket : Packet<ClickWindowPacket>
	{
		public int StateId { get; set; }
		public byte WindowId { get; set; }
		public short Slot { get; set; }

		public byte Button { get; set; }

		// public short Action { get; set; }
		public TransactionMode Mode { get; set; }
		public SlotData ClickedItem { get; set; }
		public SlotEntry[] Slots { get; set; } = new SlotEntry[0];

		public ClickWindowPacket()
		{
			PacketId = 0x08;
		}

		public override void Decode(MinecraftStream stream)
		{
			throw new System.NotImplementedException();
		}

		public override void Encode(MinecraftStream stream)
		{
			stream.WriteByte(WindowId);
			stream.WriteVarInt(StateId);
			stream.WriteShort(Slot);
			stream.WriteByte(Button);
			//    stream.WriteShort(Action);
			stream.WriteVarInt((int)Mode);

			if (Slots != null)
			{
				stream.WriteVarInt(Slots.Length);

				for (int i = 0; i < Slots.Length; i++)
				{
					var slot = Slots[i];
					stream.WriteShort(slot.SlotNumber);
					stream.WriteSlot(slot.Data);
				}
			}
			else
			{
				stream.WriteVarInt(0);
			}

			stream.WriteSlot(ClickedItem);
		}

		public enum TransactionMode
		{
			Click = 0,
			ShiftClick = 1,
			NumberKey = 2,
			MiddleClick = 3,
			Drop = 4,
			MouseDrag = 5,
			DoubleClick = 6
		}

		public class SlotEntry
		{
			public short SlotNumber;
			public SlotData Data;
		}
	}
}