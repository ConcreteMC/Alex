using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedMushroom : Block
	{
		public RedMushroom() : base(1032)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
