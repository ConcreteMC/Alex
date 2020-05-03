
using Alex.API.Utils;
using ItemType = Alex.API.Utils.ItemType;

namespace Alex.API.Items
{
    public interface IItem
    {
	    int MaxStackSize { get; set; }
	    ItemMaterial Material { get; set; } 
	    ItemType ItemType { get; set; }

	    string GetDisplayName();
    }
}
