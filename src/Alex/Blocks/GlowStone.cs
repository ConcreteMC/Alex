using Alex.Utils;

namespace Alex.Blocks
{
	public class GlowStone : Block
	{
		public GlowStone() : base(89, 0)
		{
			SetTexture(TextureSide.All, "glowstone");
		}
	}
}
