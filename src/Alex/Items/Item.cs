using Alex.API.Items;
using fNbt;
using MiNET.Items;
using ItemType = Alex.API.Utils.ItemType;

namespace Alex.Items
{
    public class Item : IItem
    {
	    public short Id { get; set; }
	    public short Meta { get; set; } = 0;
		public string Name { get; set; }
		public string DisplayName { get; set; }
	    public int MaxStackSize { get; set; }= 64;
		public int Count { get; set; } = 1;
        public ItemMaterial Material { get; set; } = ItemMaterial.None;
		public ItemType ItemType { get; set; }
		public NbtCompound Nbt { get; set; }
    }
}
