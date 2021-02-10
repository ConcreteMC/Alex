using System;
using Alex.API.Blocks;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Minecraft
{
	public class Ladder : Block
	{
		public Ladder() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			

			IsFullBlock = false;
			IsFullCube = false;
			
			BlockMaterial = Material.Wood;
			Hardness = 0.4f;
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
