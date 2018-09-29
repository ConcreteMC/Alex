using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Tripwire : Block
	{
		public Tripwire() : base(4792)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
