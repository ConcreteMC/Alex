using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EnderChest : Block
	{
		public EnderChest() : base(4642)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 7;
		}
	}
}
