using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PurpurSlab : Block
	{
		public PurpurSlab() : base(7260)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			LightOpacity = 255;
		}
	}
}
