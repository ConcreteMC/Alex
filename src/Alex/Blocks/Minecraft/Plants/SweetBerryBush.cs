using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class SweetBerryBush : Block
	{
		public SweetBerryBush()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}