using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class Allium : FlowerBase
	{
		public Allium()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}