using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class SeaLantern : Block
	{
		public SeaLantern() : base(6729)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;
		}
	}
}
