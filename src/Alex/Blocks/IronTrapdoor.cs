using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class IronTrapdoor : Block
	{
		public IronTrapdoor() : base(6419)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
