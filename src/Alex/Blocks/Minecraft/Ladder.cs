using System;
using Alex.API.Blocks;
using Alex.ResourcePackLib.Json;

namespace Alex.Blocks.Minecraft
{
	public class Ladder : Block
	{
		public Ladder() : base(3082)
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			

			IsFullBlock = false;
			IsFullCube = false;
			
			BlockMaterial = Material.Wood;
			Hardness = 0.4f;

		}

		public override bool CanClimb(BlockFace face)
		{
			if (BlockState.TryGetValue("facing", out var val))
			{
				BlockFace facing = Enum.Parse<BlockFace>(val);

				return facing.Opposite() == face;
			}

			return false;
		}
	}
}
