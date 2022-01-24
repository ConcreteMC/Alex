using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft
{
	public class DragonEgg : Block
	{
		public DragonEgg() : base()
		{
			Solid = true;
			Transparent = true;
			Luminance = 1;

			BlockMaterial = Material.DragonEgg;
		}
	}
}