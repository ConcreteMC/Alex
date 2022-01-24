using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class EndPortal : Block
	{
		public EndPortal() : base()
		{
			Solid = false;
			Transparent = true;
			Luminance = 15;

			BlockMaterial = Material.Portal;
		}
	}
}