namespace Alex.Blocks.Minecraft
{
	public class FlowingWater : Block
	{
		public FlowingWater(byte meta = 0) : base(9, meta)
		{
			Solid = false;
			Transparent = true;

			LightOpacity = 2;
			//BlockModel = new LiquidBlockModel()
			//{
			//	IsFlowing = true,
			//	Level = meta
			//};
		}
	}
}