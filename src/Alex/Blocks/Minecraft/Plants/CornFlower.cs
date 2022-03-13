using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class CornFlower : FlowerBase
	{
		public CornFlower()
		{
			Transparent = true;
			Solid = false;
			IsFullCube = false;

			BlockMaterial = Material.Plants;
		}
	}
}