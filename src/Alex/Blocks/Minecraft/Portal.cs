using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Portal : Block
	{
		public Portal() : base()
		{
			Solid = false;
			Transparent = true;

			Luminance = 11;

			BlockMaterial = Material.Portal;
		}
	}
}
