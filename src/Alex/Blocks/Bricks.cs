using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Bricks : Block
	{
		public Bricks() : base(1035)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 16;
		}
	}
}
