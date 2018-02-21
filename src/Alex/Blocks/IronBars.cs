using System.Linq;
using Alex.Graphics.Models;
using Alex.Utils;
using ResourcePackLib.Json.BlockStates;

namespace Alex.Blocks
{
    public class IronBars : Block
    {
        public IronBars() : base(101, 0)
        {
            Transparent = true;
            SetTexture(TextureSide.All, "iron_bars");
		}
    }
}
