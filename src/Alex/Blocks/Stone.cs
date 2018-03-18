using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Stone : Block
	{
		public Stone() : base(1)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
