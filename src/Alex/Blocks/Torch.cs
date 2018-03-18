using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Torch : Block
	{
		public Torch() : base(1040)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 14;
		}
	}
}
