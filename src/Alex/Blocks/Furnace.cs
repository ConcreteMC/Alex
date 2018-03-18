using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Furnace : Block
	{
		public Furnace() : base(2978)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
