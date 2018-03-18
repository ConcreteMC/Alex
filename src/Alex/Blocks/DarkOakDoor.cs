using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class DarkOakDoor : Door
	{
		public DarkOakDoor() : base(7854)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
