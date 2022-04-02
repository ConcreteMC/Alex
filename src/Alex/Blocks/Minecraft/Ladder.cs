using System;
using Alex.Blocks.Materials;
using Alex.Common.Blocks;
using Alex.Interfaces;

namespace Alex.Blocks.Minecraft
{
	public class Ladder : Block
	{
		public Ladder() : base()
		{
			Solid = true;
			Transparent = true;


			IsFullCube = false;

			BlockMaterial = Material.Wood.Clone().WithHardness(0.4f).SetCollisionBehavior(BlockCollisionBehavior.VerticalClimb);
			//Hardness = 0.4f;
			HasHitbox = true;
		}

		public override bool CanClimb(BlockFace face)
		{
			if (BlockState.TryGetValue("facing", out var val))
			{
				if (Enum.TryParse<BlockFace>(val, true, out var facing))
				{
					return facing == face;
				}
			}

			return false;
		}
	}
}