using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class AcaciaFenceGate : Block
	{
		public AcaciaFenceGate() : base(7370)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
