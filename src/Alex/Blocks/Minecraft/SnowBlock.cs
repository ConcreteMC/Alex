using Alex.Common.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class SnowBlock : Block
	{
		public SnowBlock()
		{
			Solid = true;
			
			BlockMaterial = Material.Snow.Clone().SetTintType(TintType.Color, Color.Snow);
		}
	}
}