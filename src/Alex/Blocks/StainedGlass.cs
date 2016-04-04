using Alex.Graphics.Items;
using Alex.Utils;

namespace Alex.Blocks
{
	class StainedGlass : Block
	{
		public StainedGlass(byte metadata) : base(95, metadata)
		{
			//Solid = false;
			switch (metadata)
			{
				case 0:
					SetTexture(TextureSide.All, "glass_white");
					break;
				case 1:
					SetTexture(TextureSide.All, "glass_orange");
					break;
				case 2:
					SetTexture(TextureSide.All, "glass_magenta");
					break;
				case 3:
					SetTexture(TextureSide.All, "glass_light_blue");
					break;
				case 4:
					SetTexture(TextureSide.All, "glass_yellow");
					break;
				case 5:
					SetTexture(TextureSide.All, "glass_lime");
					break;
				case 6:
					SetTexture(TextureSide.All, "glass_pink");
					break;
				case 7:
					SetTexture(TextureSide.All, "glass_gray");
					break;
				case 8:
					SetTexture(TextureSide.All, "glass_light_gray");
					break;
				case 9:
					SetTexture(TextureSide.All, "glass_cyan");
					break;
				case 10:
					SetTexture(TextureSide.All, "glass_purple");
					break;
				case 11:
					SetTexture(TextureSide.All, "glass_blue");
					break;
				case 12:
					SetTexture(TextureSide.All, "glass_brown");
					break;
				case 13:
					SetTexture(TextureSide.All, "glass_green");
					break;
				case 14:
					SetTexture(TextureSide.All, "glass_red");
					break;
				case 15:
					SetTexture(TextureSide.All, "glass_black");
					break;
			}

			Transparent = true;
		}
	}
}
