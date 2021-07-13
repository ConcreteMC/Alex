using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class PackedIce : Block
	{
		public PackedIce() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.PackedIce;

			Diffusion = 4;
		}
	}
}
