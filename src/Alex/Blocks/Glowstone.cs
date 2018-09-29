using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Glowstone : Block
	{
		public Glowstone() : base(3405)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;
		}
	}
}
