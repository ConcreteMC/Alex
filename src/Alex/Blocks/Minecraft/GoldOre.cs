using Alex.Blocks.Materials;
using Alex.Common.Items;

namespace Alex.Blocks.Minecraft
{
	public class GoldOre : Block
	{
		public GoldOre() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ore.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}