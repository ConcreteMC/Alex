using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class CoalBlock : Block
	{
		public CoalBlock() : base(6750)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
