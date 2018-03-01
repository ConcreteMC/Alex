using Alex.Graphics.Models;

namespace Alex.Blocks
{
	public class Lava : Block
	{
		public Lava(byte meta = 0) : base(10, meta)
		{
			Solid = false;
			Transparent = true;

			BlockModel = new LiquidBlockModel()
			{
				IsLava = true,
				Level = meta
			};
		}
	}

	public class FlowingLava : Block
	{
		public FlowingLava(byte meta = 0) : base(11, meta)
		{
			Solid = false;
			Transparent = true;

			BlockModel = new LiquidBlockModel()
			{
				IsFlowing = true,
				IsLava = true,
				Level = meta
			};
		}
	}
}
