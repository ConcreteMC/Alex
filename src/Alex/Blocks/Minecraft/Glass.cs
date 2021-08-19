using Alex.Blocks.Materials;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class Glass : Block
	{
		public Glass() : base()
		{
			Solid = true;
			Transparent = true;
			base.IsFullCube = true;

			base.BlockMaterial = Material.Glass;
		}
	}

	public class StainedGlass : Glass
	{
		public readonly BlockColor Color;
		public StainedGlass(BlockColor color = BlockColor.White)
		{
			Color = color;
			
			Solid = true;
			Transparent = true;
			base.IsFullCube = true;

			base.BlockMaterial = Material.Glass.Clone().WithMapColor(color.ToMapColor().WithAlpha(64));
		}
	}
}
