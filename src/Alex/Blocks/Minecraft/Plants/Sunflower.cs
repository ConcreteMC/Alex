using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class Sunflower : FlowerBase
	{
		public Sunflower()
		{
			Transparent = true;
			Solid = false;

			BlockMaterial = Material.Plants;
		}
	}
}