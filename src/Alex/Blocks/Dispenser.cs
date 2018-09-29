using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Dispenser : Block
	{
		public Dispenser() : base(144)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
