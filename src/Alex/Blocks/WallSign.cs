using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class WallSign : Block
	{
		public WallSign() : base(3180)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
