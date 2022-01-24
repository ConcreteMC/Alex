using Alex.Blocks.Materials;

namespace Alex.Blocks.Minecraft.Liquid
{
	public class FlowingLava : Lava
	{
		public FlowingLava(byte meta = 0) : base()
		{
			Solid = false;
			Transparent = true;

			BlockMaterial = Material.Lava;

			Luminance = 15;
			//BlockModel = new LiquidBlockModel()
			//{
			//	IsFlowing = true,
			//	IsLava = true,
			//	Level = meta
			//};
		}
	}
}