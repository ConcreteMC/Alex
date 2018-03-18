using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DarkOakFenceGate : Block
	{
		public DarkOakFenceGate() : base(7402)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
