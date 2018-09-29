using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class FlowerPot : Block
	{
		public FlowerPot() : base(5175)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
