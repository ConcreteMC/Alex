using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Glowstone : Block
	{
		public Glowstone() : base()
		{
			Solid = true;
			Transparent = false;
			Luminance = 15;

			BlockMaterial = Material.Glass;
		}
	}
}
