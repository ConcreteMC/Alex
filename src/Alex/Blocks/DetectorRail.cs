using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DetectorRail : Block
	{
		public DetectorRail() : base(932)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
