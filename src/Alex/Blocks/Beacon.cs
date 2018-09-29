using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Beacon : Block
	{
		public Beacon() : base(5046)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;
		}
	}
}
