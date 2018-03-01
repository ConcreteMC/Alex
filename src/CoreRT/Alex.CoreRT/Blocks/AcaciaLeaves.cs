using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
    public class AcaciaLeaves : Block
    {
        public AcaciaLeaves() : base(161, 0)
        {
	        SetColor(TextureSide.All, new Color(138, 185, 90));
	        Transparent = true;
        }
    }
}
