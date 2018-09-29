using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StoneSlab : Block
	{
		public StoneSlab() : base(7206)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
