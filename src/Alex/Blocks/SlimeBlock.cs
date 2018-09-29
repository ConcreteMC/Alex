using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class SlimeBlock : Block
	{
		public SlimeBlock() : base(6402)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = true;
		}
	}
}
