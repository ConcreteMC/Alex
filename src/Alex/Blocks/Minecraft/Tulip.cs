using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Tulip : FlowerBase
	{
		public Tulip()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}