using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class StructureBlock : Block
	{
		public StructureBlock() : base(8378)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			IsFullBlock = true;
			IsFullCube = true;
			LightOpacity = 255;
		}
	}
}
