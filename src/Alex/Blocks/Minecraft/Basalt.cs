using Alex.Blocks.Materials;
using Alex.Common.Items;

namespace Alex.Blocks.Minecraft
{
	public class Basalt : Block
	{
		public Basalt()
		{
			base.BlockMaterial = Material.Stone.Clone().WithHardness(1.25f).SetRequiresTool()
			   .SetRequiredTool(ItemType.PickAxe, ItemMaterial.AnyMaterial);
		}
	}

	public class PolishedBasalt : Basalt
	{
		
	}

}