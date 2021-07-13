using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class CrimsonStem : Block
	{
		public CrimsonStem()
		{
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.CrimsonStem);
		}
	}
}