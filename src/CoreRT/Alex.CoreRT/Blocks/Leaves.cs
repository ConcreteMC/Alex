using Alex.CoreRT.Utils;
using Microsoft.Xna.Framework;

namespace Alex.CoreRT.Blocks
{
	public class Leaves : Block
	{
		public Leaves(byte metadata) : base(18, metadata)
		{
			switch (metadata)
			{
				case 0:
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
				case 1:
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
				case 2:
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
				case 3:
					SetColor(TextureSide.All, new Color(138, 185, 90));
					break;
			}
			//Solid = false;
			Transparent = true;
		}
	}
}
