using Alex.Utils;

namespace Alex.Blocks
{
	public class EndStone:Block
	{
		public EndStone() : base(121,0)
		{
			SetTexture(TextureSide.All, "end_stone");
		}
	}
}
