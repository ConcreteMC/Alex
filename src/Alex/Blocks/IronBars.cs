using Alex.Graphics.Items;
using Alex.Graphics.Models;
using Alex.Utils;

namespace Alex.Blocks
{
    public class IronBars : Block
    {
        public IronBars() : base(101, 0)
        {
            Transparent = true;
            BlockModel = new IronBarsModel();
            SetTexture(TextureSide.All, "iron_bars");
        }
    }
}
