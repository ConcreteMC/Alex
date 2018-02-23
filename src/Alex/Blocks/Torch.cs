using System.Linq;
using Alex.Graphics.Models;
using Microsoft.Xna.Framework;
using ResourcePackLib.Json.BlockStates;

namespace Alex.Blocks
{
    public class Torch : Block
    {
        public Torch() : base(50, 0)
        {
	        Solid = false;
		}
    }
}
