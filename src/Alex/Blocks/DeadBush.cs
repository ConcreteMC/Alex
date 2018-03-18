using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DeadBush : Block
	{
		public DeadBush() : base(953)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
