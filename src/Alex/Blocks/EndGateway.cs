using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EndGateway : Block
	{
		public EndGateway() : base(8073)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 15;
		}
	}
}
