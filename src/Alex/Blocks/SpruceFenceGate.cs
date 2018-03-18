using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class SpruceFenceGate : Block
	{
		public SpruceFenceGate() : base(7274)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
