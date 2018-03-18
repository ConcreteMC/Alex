using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedSandstoneSlab : Block
	{
		public RedSandstoneSlab() : base(7254)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			LightOpacity = 255;
		}
	}
}
