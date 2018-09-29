using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StoneBrickStairs : Block
	{
		public StoneBrickStairs() : base(4333)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
