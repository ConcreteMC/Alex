using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EndStone : Block
	{
		public EndStone() : base(4544)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
