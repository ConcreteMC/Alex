using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class SandstoneStairs : Block
	{
		public SandstoneStairs() : base(4571)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
