using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PurpurBlock : Block
	{
		public PurpurBlock() : base(7983)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
