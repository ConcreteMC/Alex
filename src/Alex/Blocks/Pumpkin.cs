using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Pumpkin : Block
	{
		public Pumpkin() : base(3402)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
