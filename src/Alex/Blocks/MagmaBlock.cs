using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class MagmaBlock : Block
	{
		public MagmaBlock() : base(8102)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 255;
			LightValue = 3;
		}
	}
}
