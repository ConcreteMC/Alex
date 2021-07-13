using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class CrimsonFungus : Block
	{
		public CrimsonFungus()
		{
			Transparent = true;
			Solid = false;

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.CrimsonStem);
		}
	}
}