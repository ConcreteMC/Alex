using Alex.API.Utils;

namespace Alex.Blocks.Minecraft
{
	public class DiamondOre : Block
	{
		public DiamondOre() : base()
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Ore.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Iron);
		}
	}
}
