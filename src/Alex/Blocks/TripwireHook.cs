using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class TripwireHook : Block
	{
		public TripwireHook() : base(4658)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
