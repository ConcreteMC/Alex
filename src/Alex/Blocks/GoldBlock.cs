using Alex.Graphics.Items;
using Alex.Utils;

namespace Alex.Blocks
{
	public class GoldBlock : Block
	{
		public GoldBlock() : base(41, 0)
		{
			SetTexture(TextureSide.All, "gold_block");
		}
	}
}
