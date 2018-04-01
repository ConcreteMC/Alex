using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class IronDoor : Door
	{
		public IronDoor() : base(3224)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			CanOpen = false;
		}
	}
}
