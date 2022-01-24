using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Air : Block
	{
		public Air() : base()
		{
			Solid = false;
			Transparent = true;
			Renderable = false;
			HasHitbox = false;

			base.BlockMaterial = Material.Air;
			Diffusion = 1;
		}
	}
}