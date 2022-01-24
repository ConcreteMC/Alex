using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class Sandstone : Block
	{
		public Sandstone() : base()
		{
			Solid = true;
			Transparent = false;

			BlockMaterial = Material.Stone.Clone().WithHardness(0.8f).WithMapColor(MapColor.Sand);
		}
	}
}