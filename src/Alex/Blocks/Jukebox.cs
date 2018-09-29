using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Jukebox : Block
	{
		public Jukebox() : base(3369)
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
