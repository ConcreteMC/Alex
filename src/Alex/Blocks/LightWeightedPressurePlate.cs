using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LightWeightedPressurePlate : Block
	{
		public LightWeightedPressurePlate() : base(5513)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
