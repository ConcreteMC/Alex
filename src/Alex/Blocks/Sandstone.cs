using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Sandstone : Block
	{
		public Sandstone() : base(155)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
