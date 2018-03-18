using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StonePressurePlate : Block
	{
		public StonePressurePlate() : base(3212)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
