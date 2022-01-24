using Alex.Blocks.Materials;
using Alex.Common.Items;

namespace Alex.Blocks.Minecraft
{
	public class CoalOre : Block
	{
		public CoalOre() : base()
		{
			Solid = true;
			Transparent = false;

			this.BlockMaterial = Material.Ore.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Wood);
		}
	}
}