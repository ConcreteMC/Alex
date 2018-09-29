using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class BirchDoor : Door
	{
		public BirchDoor() : base(7662)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
