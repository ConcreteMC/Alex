using Alex.Utils;
using Alex.Worlds;

namespace Alex.Blocks
{
	public class Cobweb : Block
	{
		public Cobweb() : base(950)
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;
			IsFullBlock = false;
			IsFullCube = false;
			LightOpacity = 1;

			BlockMaterial = Material.Web;
		}
	}
}
