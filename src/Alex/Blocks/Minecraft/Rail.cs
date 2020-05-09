using Alex.API.Utils;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
	public class Rail : Block
	{
		public Rail() : base(3089)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			RequiresUpdate = true;

			Hardness = 0.7f;
		}
	}
}
