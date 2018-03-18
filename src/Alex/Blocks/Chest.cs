using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Chest : Block
	{
		public Chest() : base(1639)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
