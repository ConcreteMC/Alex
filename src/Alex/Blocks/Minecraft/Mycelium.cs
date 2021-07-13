using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Mycelium : Block
	{
		public Mycelium() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Grass;
		}
	}
}
