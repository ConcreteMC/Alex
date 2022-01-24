using Alex.Blocks.Materials;
using Alex.Common.Items;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class GoldBlock : Block
	{
		public GoldBlock() : base()
		{
			Solid = true;
			Transparent = false;

			base.BlockMaterial = Material.Metal.Clone().WithMapColor(MapColor.Gold)
			   .SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}