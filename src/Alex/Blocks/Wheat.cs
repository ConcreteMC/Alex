using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Wheat : Block
	{
		public Wheat() : base(2961)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
