using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class IronBlock : Block
	{
		public IronBlock() : base(1034)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
