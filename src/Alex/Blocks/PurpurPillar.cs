using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PurpurPillar : Block
	{
		public PurpurPillar() : base(7985)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 16;
		}
	}
}
