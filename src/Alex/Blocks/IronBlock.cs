using Alex.Utils;

namespace Alex.Blocks
{
	public class IronBlock : Block
	{
		public IronBlock() : base(42, 0)
		{
			SetTexture(TextureSide.All, "iron_block");
		}
	}
}
