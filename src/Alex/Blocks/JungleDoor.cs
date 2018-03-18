using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class JungleDoor : Block
	{
		public JungleDoor() : base(7726)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
