using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class CrimsonHyphae : Block
	{
		public CrimsonHyphae()
		{
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.CrimsonHyphae);
		}
	}
}