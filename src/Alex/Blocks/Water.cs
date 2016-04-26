using Alex.Graphics.Items;
using Alex.Utils;

namespace Alex.Blocks
{
    public class Water : Block
    {
        public Water() : base(8, 0)
        {
            Solid = false;
            Transparent = true;
            SetTexture(TextureSide.All, "water_overlay");
        }
    }
}
