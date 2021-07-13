using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class Netherrack : Block
	{
		public Netherrack() : base()
		{
			Solid = true;
			Transparent = false;
			BlockMaterial = Material.Stone.Clone().WithMapColor(MapColor.Nether).WithHardness(0.4f);
		}
	}
}
