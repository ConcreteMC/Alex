using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class MelonBlock : Block
	{
		public MelonBlock() : base(4153)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
