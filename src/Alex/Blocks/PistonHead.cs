using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PistonHead : Block
	{
		public PistonHead() : base(971)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
