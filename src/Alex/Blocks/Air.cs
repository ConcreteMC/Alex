using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Air : Block
	{
		public Air() : base(0)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = true;
			Renderable = false;
			HasHitbox = false;

			BlockMaterial = Material.Air;
			LightOpacity = 0;
		}
	}
}
