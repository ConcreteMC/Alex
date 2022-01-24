using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class Fire : Block
	{
		public Fire() : base()
		{
			Solid = false;
			Transparent = true;

			Luminance = 15;

			BlockMaterial = Material.Fire;
		}
	}
}