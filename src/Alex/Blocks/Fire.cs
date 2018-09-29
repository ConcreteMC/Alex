using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Fire : Block
	{
		public Fire() : base(1076)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
			LightValue = 15;

			BlockMaterial = Material.Fire;
		}
	}
}
