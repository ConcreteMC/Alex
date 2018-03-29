using Alex.Graphics.Models;
using Alex.Graphics.Models.Blocks;

namespace Alex.Blocks
{
	public class FlowingLava : Block
	{
		public FlowingLava(byte meta = 0) : base(11, meta)
		{
			Solid = false;
			Transparent = true;

			//BlockModel = new LiquidBlockModel()
			//{
			//	IsFlowing = true,
			//	IsLava = true,
			//	Level = meta
			//};
		}
	}
}