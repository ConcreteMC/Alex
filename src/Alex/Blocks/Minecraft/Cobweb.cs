using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Cobweb : Block
	{
		public Cobweb() : base()
		{
			Solid = false;
			Transparent = true;
			IsFullCube = false;
			Diffusion = 2;

			BlockMaterial = Material.Web;
		}
	}
}
