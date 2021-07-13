using Alex.Blocks.Materials;
using Alex.Common.Utils;
using Alex.Items;

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
