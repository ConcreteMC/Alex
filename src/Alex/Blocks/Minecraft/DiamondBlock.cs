using Alex.Blocks.Materials;
using Alex.Common.Items;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class DiamondBlock : Block
	{
		public DiamondBlock() : base()
		{
			Solid = true;
			Transparent = false;
			base.BlockMaterial = Material.Metal.Clone().WithMapColor(MapColor.Diamond).SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}
