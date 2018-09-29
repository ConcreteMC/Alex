using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class JungleFenceGate : FenceGate
	{
		public JungleFenceGate() : base(7338)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
