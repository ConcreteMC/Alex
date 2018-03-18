using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BirchDoor : Block
	{
		public BirchDoor() : base(7662)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
