using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DaylightDetector : Block
	{
		public DaylightDetector() : base(5577)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
