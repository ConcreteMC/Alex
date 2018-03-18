using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BrownMushroomBlock : Block
	{
		public BrownMushroomBlock() : base(3897)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
