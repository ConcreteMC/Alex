using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Fern : Block
	{
		public Fern() : base()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;
			BlockMaterial = Material.Plants;
		}
	}
}