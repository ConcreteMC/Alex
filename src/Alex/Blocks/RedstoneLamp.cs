using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class RedstoneLamp : Block
	{
		public RedstoneLamp() : base(4547)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
