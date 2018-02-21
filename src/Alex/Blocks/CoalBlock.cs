using Alex.Utils;

namespace Alex.Blocks
{
	public class CoalBlock : Block
	{
		public CoalBlock() : base(173, 0)
		{
			SetTexture(TextureSide.All, "coal_block");
		}
	}
}
