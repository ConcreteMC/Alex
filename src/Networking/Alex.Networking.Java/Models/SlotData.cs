using fNbt;

namespace Alex.Networking.Java.Models
{
	public class SlotData
	{
		public int ItemID = -1;
		public byte Count = 0;

		public NbtCompound Nbt = null;

		public SlotData() { }
	}
}