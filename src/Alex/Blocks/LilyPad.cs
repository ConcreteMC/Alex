using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LilyPad : Block
	{
		public LilyPad() : base(4404)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
