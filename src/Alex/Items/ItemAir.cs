using Alex.Common.Utils;

namespace Alex.Items
{
    public class ItemAir : Item
    {
	    public ItemAir()
	    {
		    Material = ItemMaterial.None;
		    ItemType = ItemType.Hand;
		    
		    Name = "minecraft:air";
		    DisplayName = "Air";
	    }
    }
}
