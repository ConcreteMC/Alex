using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class FrostedIce : Block
	{
		public FrostedIce() : base()
		{
			Solid = true;
			Transparent = true;

			BlockMaterial = Material.Ice;
		}
	}
}