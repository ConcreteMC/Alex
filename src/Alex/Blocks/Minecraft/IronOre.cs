using Alex.Blocks.Materials;
using Alex.Common.Items;

namespace Alex.Blocks.Minecraft
{
	public class IronOre : Block
	{
		public IronOre() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ore.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}