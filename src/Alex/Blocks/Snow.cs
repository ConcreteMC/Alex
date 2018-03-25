using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Snow : Block
	{
		public Snow() : base(3325)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = false;
			//BlockMaterial = Material.Snow;
		}
	}
}
