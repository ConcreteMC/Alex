using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Peony : FlowerBase
	{
		public Peony()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}