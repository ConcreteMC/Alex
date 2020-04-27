using Alex.API.Items;
using Alex.API.Utils;
using Alex.Graphics.Models.Items;
using fNbt;
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
		public IItemRenderer Renderer { get; set; }

		public virtual Item Clone()
		{
			return new Item()
			{
				Count = Count,
				Id = Id,
				Material = Material,
				Meta = Meta,
				Name = Name,
				Nbt = Nbt,
				Renderer = Renderer,
				DisplayName = DisplayName,
				ItemType = ItemType,
				MaxStackSize = MaxStackSize
			};
		}
    }
}
