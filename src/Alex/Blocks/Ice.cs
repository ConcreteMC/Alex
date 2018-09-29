using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Ice : Block
	{
		public Ice() : base(3333)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

		//	BlockMaterial = Material.Ice;
		}
	}
}
