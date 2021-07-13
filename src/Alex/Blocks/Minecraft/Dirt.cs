using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Dirt : Block
	{
		public Dirt() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Dirt;
		}
	}
}
