using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Netherrack : Block
	{
		public Netherrack() : base()
		{
			Solid = true;
			Transparent = false;
			BlockMaterial = Material.Stone.WithHardness(0.4f);
		}
	}
}
