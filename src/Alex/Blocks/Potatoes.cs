using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Potatoes : Block
	{
		public Potatoes() : base(5205)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
