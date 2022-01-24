using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class NetherWart : Block
	{
		public NetherWart() : base()
		{
			Solid = false;
			Transparent = true;

			IsFullCube = false;

			base.BlockMaterial = Material.Wood.Clone().WithMapColor(MapColor.Nether);
		}
	}
}