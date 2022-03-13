using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class OxeyeDaisy : FlowerBase
	{
		public OxeyeDaisy()
		{
			Transparent = true;
			Solid = false;

			BlockMaterial = Material.Plants;
		}
	}
}