using Alex.Utils;

namespace Alex.Blocks
{
	public class Glass : Block
	{
		public Glass() : base(20, 0)
		{
			//Solid = false;
			Transparent = true;
			SetTexture(TextureSide.All, "glass");
		}
	}
}
