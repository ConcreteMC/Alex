using Alex.Blocks.Materials;
using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class GlowLichen : Block
	{
		public GlowLichen()
		{
			base.Solid = false;
			base.Luminance = 7;
			base.Transparent = true;
			base.IsFullCube = false;

			base.BlockMaterial = new Material(MapColor.Air).WithHardness(0.2f).SetReplaceable().SetWaterLoggable().SetFlammable();
		}
	}
}