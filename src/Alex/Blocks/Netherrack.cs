using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Netherrack : Block
	{
		public Netherrack() : base(3403)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
		}
	}
}
