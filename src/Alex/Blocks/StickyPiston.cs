using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StickyPiston : Block
	{
		public StickyPiston() : base(944)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
