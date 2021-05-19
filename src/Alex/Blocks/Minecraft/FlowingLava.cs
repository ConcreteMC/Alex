namespace Alex.Blocks.Minecraft
{
	public class FlowingLava : Lava
	{
		public FlowingLava(byte meta = 0) : base()
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