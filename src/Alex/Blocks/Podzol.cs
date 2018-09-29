using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Podzol : Block
	{
		public Podzol() : base(13)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
