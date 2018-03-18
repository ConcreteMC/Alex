using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BirchFenceGate : Block
	{
		public BirchFenceGate() : base(7306)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
