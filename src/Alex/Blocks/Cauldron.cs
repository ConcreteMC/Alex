using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Cauldron : Block
	{
		public Cauldron() : base(4531)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
