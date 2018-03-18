using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Carrots : Block
	{
		public Carrots() : base(5197)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
