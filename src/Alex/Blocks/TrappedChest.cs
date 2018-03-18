using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class TrappedChest : Block
	{
		public TrappedChest() : base(5490)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
		}
	}
}
