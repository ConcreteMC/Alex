using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedstoneTorch : Block
	{
		public RedstoneTorch() : base(3291)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 7;
		}
	}
}
