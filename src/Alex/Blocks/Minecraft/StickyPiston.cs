using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class StickyPiston : Block
	{
		public StickyPiston() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Piston;
		}
	}
}
