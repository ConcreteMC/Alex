using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class LapisOre : Block
	{
		public LapisOre() : base(141)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
