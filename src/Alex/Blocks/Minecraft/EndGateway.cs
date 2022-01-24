using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class EndGateway : Block
	{
		public EndGateway() : base()
		{
			Solid = true;
			Transparent = false;
			Luminance = 15;

			BlockMaterial = Material.Portal;
		}
	}
}