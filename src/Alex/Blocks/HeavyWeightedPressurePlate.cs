using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class HeavyWeightedPressurePlate : Block
	{
		public HeavyWeightedPressurePlate() : base(5529)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
