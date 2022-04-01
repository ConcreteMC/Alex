using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Stone : Block
	{
		public Stone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().WithHardness(1.5f);
		}
	}
}