using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Hopper : Block
	{
		public Hopper() : base(5595)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
