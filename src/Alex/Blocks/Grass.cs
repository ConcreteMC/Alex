using Alex.Graphics.Items;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
    public class Grass : Block
    {
        public Grass() : base(2, 0)
        {
            SetTexture(TextureSide.Side, "grass_side");
            SetTexture(TextureSide.Top, "grass_top");
            SetTexture(TextureSide.Bottom, "dirt");
            SetColor(TextureSide.Top, new Color(138, 185, 90));
        }
    }
}
