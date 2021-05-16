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
			IsFullCube = false;
			
			BlockMaterial = Material.Snow.Clone().SetTintType(TintType.Color, Color.Snow).SetHardness(0.2f);
		}
	}
}
