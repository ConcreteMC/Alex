using Alex.Graphics.Models;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks
{
	public class FlowingWater : Block
	{
		public FlowingWater(byte meta = 0) : base(9, meta)
		{
			Solid = false;
			Transparent = true;

			//BlockModel = new LiquidBlockModel()
			//{
			//	IsFlowing = true,
			//	Level = meta
			//};
		}
	}
}