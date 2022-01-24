using Alex.Blocks.Materials;
using Alex.Common.Items;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class Granite : Block
	{
		public Granite()
		{
			Solid = true;

			base.BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Dirt).WithHardness(1.5f)
			   .SetRequiredTool(ItemType.PickAxe, ItemMaterial.Any).SetRequiresTool();
		}
	}

	public class PolishedGranite : Granite
	{
		public PolishedGranite() { }
	}
}