using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Lever : Block
	{
		public Lever() : base(3196)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
