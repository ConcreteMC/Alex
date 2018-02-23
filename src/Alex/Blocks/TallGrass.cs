using System.Linq;
using Alex.Graphics.Models;
using Alex.Utils;
using Microsoft.Xna.Framework;
using ResourcePackLib.Json.BlockStates;

namespace Alex.Blocks
{
	public class TallGrass : Block
	{
		public TallGrass() : base(31, 0)
		{
		//	BlockModel = new GrassModel();

			Transparent = true;
			Solid = false;
			SetColor(TextureSide.All, new Color(138, 185, 90));
		}
	}
}
