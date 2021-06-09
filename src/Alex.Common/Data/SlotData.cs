using fNbt;

namespace Alex.Common.Data
{
    public class SlotData
    {
	    public int ItemID = -1;
	    public short ItemDamage = 0;
	    public byte Count = 0;

		public NbtCompound Nbt = null;

		public SlotData()
	    {
		    
	    }
    }
}
