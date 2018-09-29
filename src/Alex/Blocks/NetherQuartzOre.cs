using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class NetherQuartzOre : Block
	{
		public NetherQuartzOre() : base(5594)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 255;
		}
	}
}
