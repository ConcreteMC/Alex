using Alex.Utils;

namespace Alex.Blocks
{
    public class StainedClay : Block
    {
        public StainedClay(byte metadata) : base(159, metadata)
        {
            string texture = "hardened_clay";
            switch (metadata)
            {
                case 1:
                    texture = "hardened_clay_stained_orange";
                    break;
                case 2:
                    texture = "hardened_clay_stained_magenta";
                    break;
                case 3:
                    texture = "hardened_clay_stained_light_blue";
                    break;
                case 4:
                    texture = "hardened_clay_stained_yellow";
                    break;
                case 5:
                    texture = "hardened_clay_stained_lime";
                    break;
                case 6:
                    texture = "hardened_clay_stained_pink";
                    break;
                case 7:
                    texture = "hardened_clay_stained_gray";
                    break;
                case 8:
                    texture = "hardened_clay_stained_light_gray";
                    break;
                case 9:
                    texture = "hardened_clay_stained_cyan";
                    break;
                case 10:
                    texture = "hardened_clay_stained_purple";
                    break;
                case 11:
                    texture = "hardened_clay_stained_blue";
                    break;
                case 12:
                    texture = "hardened_clay_stained_brown";
                    break;
                case 13:
                    texture = "hardened_clay_stained_green";
                    break;
                case 14:
                    texture = "hardened_clay_stained_red";
                    break;
                case 15:
                    texture = "hardened_clay_stained_black";
                    break;
            }
			SetTexture(TextureSide.All, texture);
        }
    }
}
