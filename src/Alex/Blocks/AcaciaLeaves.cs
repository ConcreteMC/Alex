using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
    public class AcaciaLeaves : Block
    {
        public AcaciaLeaves() : base(161, 0)
        {
            SetTexture(TextureSide.All, "leaves_acacia");
            SetColor(TextureSide.All, new Color(138, 185, 90));
        }
    }
}
