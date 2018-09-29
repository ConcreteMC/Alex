using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Gravel : Block
	{
		public Gravel() : base(68)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
