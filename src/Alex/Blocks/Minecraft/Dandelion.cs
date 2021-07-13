using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Dandelion : FlowerBase
	{
		public Dandelion() : base()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}
