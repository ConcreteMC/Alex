using Alex.Graphics.Items;
using Alex.Utils;

namespace Alex.Blocks
{
    public class Stone : Block
    {
        public Stone(byte meta = 0) : base(1, meta)
        {
            SetTexture(Utils.TextureSide.All, "stone");

	        switch (meta)
	        {
				case 0:
			        break;
				case 1:
					SetTexture(TextureSide.All, "stone_granite");
			        break;
				case 2:
					SetTexture(TextureSide.All, "stone_granite_smooth");
					break;
				case 3:
					SetTexture(TextureSide.All, "stone_diorite");
					break;
				case 4:
					SetTexture(TextureSide.All, "stone_diorite_smooth");
					break;
				case 5:
					SetTexture(TextureSide.All, "stone_andesite");
					break;
				case 6:
					SetTexture(TextureSide.All, "stone_andesite_smooth");
					break;
			}
        }
    }
}
