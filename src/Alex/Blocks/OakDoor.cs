using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class OakDoor : Block
	{
		public OakDoor() : base(3028)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
		}
	}
}
