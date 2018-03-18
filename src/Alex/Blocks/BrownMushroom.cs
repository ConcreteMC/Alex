using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BrownMushroom : Block
	{
		public BrownMushroom() : base(1031)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 1;
		}
	}
}
