using Alex.Utils;

namespace Alex.Blocks.Minecraft
{
	public class GlowLichen : Block
	{
		public GlowLichen()
		{
			base.Solid = false;
			base.LightValue = 7;
			base.Transparent = true;
			base.IsFullCube = false;

			base.BlockMaterial = new Material(MapColor.AIR).SetHardness(0.2f).SetReplaceable().SetWaterLoggable().SetBurning();
		}
	}
}