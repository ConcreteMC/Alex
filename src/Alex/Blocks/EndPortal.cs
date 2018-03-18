using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EndPortal : Block
	{
		public EndPortal() : base(4535)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			LightValue = 15;
		}
	}
}
