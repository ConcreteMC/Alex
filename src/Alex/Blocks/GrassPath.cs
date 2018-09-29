using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class GrassPath : Block
	{
		public GrassPath() : base(8072)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
