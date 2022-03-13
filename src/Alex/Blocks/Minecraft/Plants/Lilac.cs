using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class Lilac : FlowerBase
	{
		public Lilac()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}