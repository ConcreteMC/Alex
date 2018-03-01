using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
    public class Grass : Block
    {
        public Grass() : base(2, 0)
        {
	        SetColor(TextureSide.Top, new Color(138, 185, 90));
			//BlockModel = ResManager.MCResourcePack.BlockStates.
		}
    }
}
