using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class RedMushroomBlock : Block
	{
		public RedMushroomBlock() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Ground.WithHardness(0.2f);
		}
	}
}
