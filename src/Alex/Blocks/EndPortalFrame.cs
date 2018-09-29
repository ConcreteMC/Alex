using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EndPortalFrame : Block
	{
		public EndPortalFrame() : base(4540)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 1;
		}
	}
}
