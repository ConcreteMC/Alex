using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class PistonHead : Block
	{
		public PistonHead() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Piston;
		}
	}
}