using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class JungleStairs : Block
	{
		public JungleStairs() : base(4965)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
