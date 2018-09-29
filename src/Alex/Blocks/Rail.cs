using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Rail : Block
	{
		public Rail() : base(3089)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
