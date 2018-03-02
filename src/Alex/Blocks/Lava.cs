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
}
