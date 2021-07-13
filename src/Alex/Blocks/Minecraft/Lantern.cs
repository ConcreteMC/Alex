using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Lantern : Block
	{
		public Lantern()
		{
			Solid = true;
			Transparent = true;

			Luminance = 15;
			
			base.BlockMaterial = Material.Metal;
		}
	}
}