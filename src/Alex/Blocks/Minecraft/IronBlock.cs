using Alex.Blocks.Materials;
using Alex.Common.Items;

namespace Alex.Blocks.Minecraft
{
	public class IronBlock : Block
	{
		public IronBlock() : base()
		{
			Solid = true;
			Transparent = false;

			base.BlockMaterial = Material.Metal.Clone().SetRequiredTool(ItemType.PickAxe, ItemMaterial.Stone);
		}
	}
}