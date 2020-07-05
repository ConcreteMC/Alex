using System.Diagnostics;
using Alex.API.Data.Servers;
using Alex.API.Items;
using Alex.API.Utils;
using Alex.Graphics.Models.Items;
using fNbt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ItemType = Alex.API.Utils.ItemType;

namespace Alex.Items
{
    public class Item
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

		public int StackID { get; set; } = -1;
		
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
				Renderer = Renderer?.Clone(),
				DisplayName = DisplayName,
				ItemType = ItemType,
				MaxStackSize = MaxStackSize
			};
		}

		public string GetDisplayName()
		{
			if (Nbt != null)
			{
				if (Nbt.TryGet("display", out NbtCompound display))
				{
					if (display.TryGet("Name", out NbtString name))
					{
						if (ValidateJSON(name.Value))
						{
							return ChatParser.ParseText(name.Value);
						}
						return name.Value;
					}
				}
			}

			return DisplayName;
		}
		
		private bool ValidateJSON(string s)
		{
			try
			{
				JToken.Parse(s);
				return true;
			}
			catch (JsonReaderException ex)
			{
				Trace.WriteLine(ex);
				return false;
			}
		}
    }
}
