using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class MobSpawner : Block
	{
		public MobSpawner() : base(1557)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 1;
		}
	}
}
