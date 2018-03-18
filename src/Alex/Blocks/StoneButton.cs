using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StoneButton : Block
	{
		public StoneButton() : base(3310)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
