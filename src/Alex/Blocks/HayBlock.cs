using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class HayBlock : Block
	{
		public HayBlock() : base(6731)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
