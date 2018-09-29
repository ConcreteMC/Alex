using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Mycelium : Block
	{
		public Mycelium() : base(4403)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
