using System.Linq;
using Alex.Graphics.Models;
using Alex.Utils;
using Microsoft.Xna.Framework;
using ResourcePackLib.Json.BlockStates;

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
