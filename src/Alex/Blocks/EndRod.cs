using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class EndRod : Block
	{
		public EndRod() : base(7911)
		{
			Solid = true;
			Transparent = false;
			IsReplacible = false;
			LightValue = 14;
		}
	}
}
