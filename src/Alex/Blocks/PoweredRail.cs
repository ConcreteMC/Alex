using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PoweredRail : Block
	{
		public PoweredRail() : base(920)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
