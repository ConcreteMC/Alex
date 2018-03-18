using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Dropper : Block
	{
		public Dropper() : base(5703)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
