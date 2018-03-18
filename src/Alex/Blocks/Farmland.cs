using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Farmland : Block
	{
		public Farmland() : base(2969)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
