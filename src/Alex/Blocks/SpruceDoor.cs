using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class SpruceDoor : Block
	{
		public SpruceDoor() : base(7598)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
