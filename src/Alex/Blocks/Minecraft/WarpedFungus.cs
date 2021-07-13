using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class WarpedFungus : Block
	{
		public WarpedFungus()
		{
			Transparent = true;
			Solid = false;

			IsFullCube = false;
			
			base.BlockMaterial = Material.Plants.Clone().WithMapColor(MapColor.WarpedStem);
		}
	}
}