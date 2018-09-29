using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Poppy : Block
	{
		public Poppy() : base(1022)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
