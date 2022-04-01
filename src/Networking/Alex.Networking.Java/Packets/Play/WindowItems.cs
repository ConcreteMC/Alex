using Alex.Networking.Java.Models;
using Alex.Networking.Java.Util;

//using fNbt.Tags;

namespace Alex.Networking.Java.Packets.Play
{
	public class WindowItems : Packet<WindowItems>
	{
		public WindowItems() { }

		public byte WindowId = 0;
		public int StateId = 0;
		public SlotData[] Slots;
		public SlotData CarriedSlot;

		public override void Decode(MinecraftStream stream)
		{
			WindowId = (byte)stream.ReadByte();

			StateId = stream.ReadVarInt();

			var slotCount = stream.ReadVarInt();
			Slots = new SlotData[slotCount];

			for (int i = 0; i < Slots.Length; i++)
			{
				Slots[i] = stream.ReadSlot();
			}

			CarriedSlot = stream.ReadSlot();
		}

		public override void Encode(MinecraftStream stream) { }
	}
}