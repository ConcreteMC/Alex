using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class IronBars : Block
	{
		public IronBars() : base(4120)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
