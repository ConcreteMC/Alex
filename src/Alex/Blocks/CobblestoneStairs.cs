using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class CobblestoneStairs : Block
	{
		public CobblestoneStairs() : base(3110)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			LightOpacity = 16;
		}
	}
}
