using Alex.API.Utils;

namespace Alex.Blocks.Minecraft
{
	public class GoldOre : Block
	{
		public GoldOre() : base(69)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			
			BlockMaterial = Material.Ore.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}
