using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class MossyCobblestone : Block
	{
		public MossyCobblestone() : base(1038)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
