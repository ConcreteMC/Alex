using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class NetherSprouts : Block
	{
		public NetherSprouts()
		{
			Transparent = true;
			Solid = false;
			
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}