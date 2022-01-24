using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class CraftingTable : Block
	{
		public CraftingTable() : base()
		{
			Solid = true;
			Transparent = false;

			CanInteract = true;

			BlockMaterial = Material.Wood.WithHardness(2.5f);
		}
	}
}