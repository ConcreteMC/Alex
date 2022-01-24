using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Liquid
{
	public class Lava : LiquidBlock
	{
		public Lava() : base()
		{
			Solid = false;
			Transparent = true;
			HasHitbox = false;

			Luminance = 15;
			Diffusion = 1;

			BlockMaterial = Material.Lava;
			//BlockModel = BlockFactory.StationairyLavaModel;

			//	BlockMaterial = Material.Lava;
		}
	}
}