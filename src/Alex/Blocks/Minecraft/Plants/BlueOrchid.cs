using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class BlueOrchid : FlowerBase
	{
		public BlueOrchid()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}