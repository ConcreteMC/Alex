using Alex.Blocks.Materials;
using Alex.Common.Blocks;
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

		/// <inheritdoc />
		public override bool ShouldRenderFace(BlockFace face, Block other)
		{
			/*if (FancyGraphics && other is Glass)
			{
				if (face == BlockFace.Up || face == BlockFace.Down)
				{
					return true;
				}
			}*/

			return base.ShouldRenderFace(face, other);
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

		/// <inheritdoc />
		public override bool ShouldRenderFace(BlockFace face, Block other)
		{
			if (FancyGraphics && other is StainedGlass stained && stained.Color != Color)
			{
				//if (face == BlockFace.Up || face == BlockFace.Down)
				{
					//	return true;
				}
			}

			return base.ShouldRenderFace(face, other);
		}
	}
}