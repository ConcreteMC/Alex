using Alex.Blocks.Materials;
using Alex.Common.Blocks;
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
			
			BlockMaterial = Material.Snow.Clone().WithTintType(TintType.Color, Color.Snow).WithHardness(0.2f);
		}
	}
}
