using System;
using Alex.Networking.Java.Models;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class SetSlot : Packet<SetSlot>
	{
		public SetSlot()
		{
			PacketId = 0x16;
		}

		public int StateId = 0;
		public byte WindowId = 0;
		public short SlotId = 0;
		public SlotData Slot;

		public override void Decode(MinecraftStream stream)
		{
			WindowId = (byte)stream.ReadByte();
			StateId = stream.ReadVarInt();
			SlotId = stream.ReadShort();
			Slot = stream.ReadSlot();
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
	}
}