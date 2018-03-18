using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Cobblestone : Block
	{
		public Cobblestone() : base(14)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
