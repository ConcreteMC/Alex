using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class NetherWartBlock : Block
	{
		public NetherWartBlock() : base()
		{
			Solid = true;
			Transparent = false;
			IsFullCube = true;
			
			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}
