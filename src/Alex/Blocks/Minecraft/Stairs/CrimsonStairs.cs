using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft.Stairs
{
	public class CrimsonStairs : Stairs
	{
		public CrimsonStairs()
		{
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}