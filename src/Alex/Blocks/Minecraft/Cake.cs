using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Cake : Block
	{
		public Cake() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Cake;
		}
	}
}
