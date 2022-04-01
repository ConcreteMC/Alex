using Alex.Blocks.Materials;
using Alex.Common.Blocks;
using Alex.Interfaces;

namespace Alex.Blocks.Minecraft
{
	public class Carpet : Block
	{
		public Carpet()
		{
			Solid = true;
			Transparent = true;

			base.BlockMaterial = Material.Carpet;
			IsFullCube = false;
		}

		/// <inheritdoc />
		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			if (face == BlockFace.Down)
			{
				if (neighbor.Solid)
				{
					if (neighbor.IsFullCube && !neighbor.Transparent)
						return false;
				}
			}

			return base.ShouldRenderFace(face, neighbor);
		}
	}
}