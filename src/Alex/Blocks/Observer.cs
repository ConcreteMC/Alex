using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Observer : Block
	{
		public Observer() : base(8114)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
