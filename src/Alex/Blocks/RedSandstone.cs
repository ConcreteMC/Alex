using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedSandstone : Block
	{
		public RedSandstone() : base(7084)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
