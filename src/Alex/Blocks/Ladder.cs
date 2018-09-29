using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Ladder : Block
	{
		public Ladder() : base(3082)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
