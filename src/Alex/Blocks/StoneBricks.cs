using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StoneBricks : Block
	{
		public StoneBricks() : base(3893)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 255;
		}
	}
}
