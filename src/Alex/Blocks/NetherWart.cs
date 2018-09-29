using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class NetherWart : Block
	{
		public NetherWart() : base(4518)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
