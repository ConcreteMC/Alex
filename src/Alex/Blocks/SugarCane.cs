using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class SugarCane : Block
	{
		public SugarCane() : base(3352)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
