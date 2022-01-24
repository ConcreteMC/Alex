using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Bamboo : Block
	{
		public Bamboo()
		{
			Solid = true;
			Transparent = true;

			IsFullCube = false;
			base.BlockMaterial = Material.Bamboo;
		}
	}
}