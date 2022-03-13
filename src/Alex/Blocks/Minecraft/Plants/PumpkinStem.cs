using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Plants
{
	public class PumpkinStem : Block
	{
		public PumpkinStem() : base()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Plants;
		}
	}
}