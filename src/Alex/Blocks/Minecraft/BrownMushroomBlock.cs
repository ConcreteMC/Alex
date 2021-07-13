using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class BrownMushroomBlock : Block
	{
		public BrownMushroomBlock() : base()
		{
			Solid = true;
			Transparent = false;
			
			BlockMaterial = Material.Ground.WithHardness(0.2f);
		}
	}
}
