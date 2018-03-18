using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class GoldOre : Block
	{
		public GoldOre() : base(69)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
