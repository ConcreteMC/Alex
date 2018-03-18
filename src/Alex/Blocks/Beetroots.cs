using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Beetroots : Block
	{
		public Beetroots() : base(8068)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
