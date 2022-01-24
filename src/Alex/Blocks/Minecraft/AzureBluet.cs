using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class AzureBluet : FlowerBase
	{
		public AzureBluet()
		{
			Transparent = true;
			Solid = false;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}