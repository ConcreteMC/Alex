using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class QuartzBlock : Block
	{
		public QuartzBlock() : base(5605)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
