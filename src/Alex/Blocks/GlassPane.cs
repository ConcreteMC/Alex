using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class GlassPane : Block
	{
		public GlassPane() : base(4152)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = false;
		}
	}
}
