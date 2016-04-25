using Alex.Graphics.Items;
using Alex.Utils;

namespace Alex.Blocks
{
	public class Planks : Block
	{
		public Planks(byte meta = 0) : base(5,0)
		{
			switch (meta)
			{
				case 1:
					SetTexture(TextureSide.All, "planks_spruce");
					break;
				case 2:
					SetTexture(TextureSide.All, "planks_birch");
					break;
				case 3:
					SetTexture(TextureSide.All, "planks_jungle");
					break;
				case 4:
					SetTexture(TextureSide.All, "planks_acacia");
					break;
				case 5:
					SetTexture(TextureSide.All, "planks_big_oak");
					break;
				default:
					SetTexture(TextureSide.All, "planks_oak");
					break;
			}
		}
	}
}
