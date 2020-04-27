using Alex.API.Utils;

namespace Alex.Blocks.Minecraft
{
	public class DiamondOre : Block
	{
		public DiamondOre() : base(2958)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Ore.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Iron);
		}
	}
}
