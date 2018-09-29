using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DarkOakStairs : Block
	{
		public DarkOakStairs() : base(6333)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
