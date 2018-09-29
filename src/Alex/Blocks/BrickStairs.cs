using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BrickStairs : Block
	{
		public BrickStairs() : base(4253)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
