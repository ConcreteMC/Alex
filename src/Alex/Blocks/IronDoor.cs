using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class IronDoor : Block
	{
		public IronDoor() : base(3224)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
