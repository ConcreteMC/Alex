using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Bookshelf : Block
	{
		public Bookshelf() : base(1037)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
