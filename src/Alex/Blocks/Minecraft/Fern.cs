using Alex.Blocks.Materials;
using Alex.Utils;

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