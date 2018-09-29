using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RepeatingCommandBlock : Block
	{
		public RepeatingCommandBlock() : base(8080)
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
