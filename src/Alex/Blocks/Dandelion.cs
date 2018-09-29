using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Dandelion : Block
	{
		public Dandelion() : base(1021)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
