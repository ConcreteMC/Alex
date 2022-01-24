using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class WarpedRoots : Block
	{
		public WarpedRoots()
		{
			Transparent = true;
			Solid = false;

			IsFullCube = false;

			base.BlockMaterial = Material.Plants.Clone().SetReplaceable().WithMapColor(MapColor.WarpedStem);
		}
	}
}