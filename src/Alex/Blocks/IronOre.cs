using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class IronOre : Block
	{
		public IronOre() : base(70)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
