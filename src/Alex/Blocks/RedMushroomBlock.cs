using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedMushroomBlock : Block
	{
		public RedMushroomBlock() : base(3961)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
