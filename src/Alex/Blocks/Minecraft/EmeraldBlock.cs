using Alex.Blocks.Materials;
using Alex.Common.Items;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class EmeraldBlock : Block
	{
		public EmeraldBlock() : base()
		{
			Solid = true;
			Transparent = false;
			
			base.BlockMaterial = Material.Metal.Clone().WithMapColor(MapColor.Emerald).SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}
