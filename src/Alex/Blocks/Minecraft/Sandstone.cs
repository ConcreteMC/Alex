using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Sandstone : Block
	{
		public Sandstone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().WithHardness(0.8f);
		}
	}
}
