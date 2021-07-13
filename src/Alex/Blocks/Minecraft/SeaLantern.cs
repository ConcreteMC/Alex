using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class SeaLantern : Block
	{
		public SeaLantern() : base()
		{
			Solid = true;
			Transparent = false;
			Luminance = 15;

			BlockMaterial = Material.Glass;
		}
	}
}
