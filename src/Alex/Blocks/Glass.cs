using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Glass : Block
	{
		public Glass() : base(140)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
