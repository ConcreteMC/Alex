namespace Alex.Blocks.Minecraft
{
	public class FlowingLava : Block
	{
		public FlowingLava(byte meta = 0) : base(11, meta)
		{
			Solid = false;
			Transparent = true;
			
			BlockMaterial = Material.Lava;

			LightValue = 15;
			//BlockModel = new LiquidBlockModel()
			//{
			//	IsFlowing = true,
			//	IsLava = true,
			//	Level = meta
			//};
		}
	}
}