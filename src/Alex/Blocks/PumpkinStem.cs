using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class PumpkinStem : Block
	{
		public PumpkinStem() : base(4162)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
