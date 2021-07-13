using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Prismarine : Block
	{
		public Prismarine() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().WithHardness(1.5f);
		}
	}
}
