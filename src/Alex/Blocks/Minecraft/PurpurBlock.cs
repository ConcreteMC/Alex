using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class PurpurBlock : Block
	{
		public PurpurBlock() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.WithHardness(1.5f);
		}
	}
}
