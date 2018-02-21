using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
	public class Leaves : Block
	{
		public Leaves(byte metadata) : base(18, metadata)
		{
			switch (metadata)
			{
				case 0:
					SetTexture(TextureSide.All, "leaves_oak");
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
				case 1:
					SetTexture(TextureSide.All, "leaves_spruce");
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
				case 2:
					SetTexture(TextureSide.All, "leaves_birch");
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
				case 3:
					SetTexture(TextureSide.All, "leaves_jungle");
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
			}
			//Solid = false;
			Transparent = true;
		}
	}
}
