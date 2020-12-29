using Alex.API.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class Snow : Block
	{
		public Snow() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			IsFullCube = false;

			Hardness = 0.2f;
			
			BlockMaterial = Material.Snow.Clone().SetTintType(TintType.Color, Color.Snow);
		}
	}
}
