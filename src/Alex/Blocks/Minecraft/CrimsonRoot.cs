using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class CrimsonRoot : Block
	{
		public CrimsonRoot()
		{
			Transparent = true;
			Solid = false;

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.CrimsonStem);
		}
	}
}