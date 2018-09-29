using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class NetherBrickStairs : Block
	{
		public NetherBrickStairs() : base(4449)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
