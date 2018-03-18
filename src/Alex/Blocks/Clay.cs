using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Clay : Block
	{
		public Clay() : base(3351)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;

			BlockMaterial = Material.Clay;
		}
	}
}
