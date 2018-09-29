using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DiamondBlock : Block
	{
		public DiamondBlock() : base(2959)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
