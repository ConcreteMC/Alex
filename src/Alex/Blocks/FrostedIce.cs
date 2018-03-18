using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class FrostedIce : Block
	{
		public FrostedIce() : base(8098)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
