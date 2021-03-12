using Alex.API.Blocks;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.API.World;
using Alex.Blocks.State;
using Alex.Worlds;
using Alex.Worlds.Abstraction;

namespace Alex.Blocks.Minecraft
{
	public class Rail : Block
	{
		public Rail() : base()
		{
			Solid = false;
			Transparent = true;
			IsReplacible = false;

			RequiresUpdate = true;
			IsFullCube = false;

			Hardness = 0.7f;
		}

		/*private bool UpdateState(IBlockAccess world,
			BlockState state,
			BlockCoordinates position,
			BlockCoordinates updatedBlock,
			out BlockState result)
		{
			result = state;
			var block = world.GetBlockState(updatedBlock).Block;

			if (!(block is Rail))
			{
				return false;
			}
			
			var blockState = block.BlockState;

			//var facing   = GetFacing(state);
			//var neighborFacing = GetFacing(blockState);

			var shape       = GetShape(state);
			var neighborShape = GetShape(blockState);

			if (updatedBlock == position + BlockCoordinates.South)
			{
				
			}
		}*/

		public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
		//	if (UpdateState(world, BlockState, position, updatedBlock, out var state))
		//	{
		//		world.SetBlockState(position.X, position.Y, position.Z, state);
		//	}
		}

		private bool Check(IBlockAccess world, BlockCoordinates position, BlockFace face, out int yOffset)
		{
			yOffset = 0;
			if (world.GetBlockState(position + face.GetBlockCoordinates()).Block is Rail)
			{
				return true;
			}

			if (world.GetBlockState(position + BlockCoordinates.Up + face.GetBlockCoordinates()).Block is Rail)
			{
				yOffset = 1;
				return true;
			}
			
			if (world.GetBlockState(position + BlockCoordinates.Down + face.GetBlockCoordinates()).Block is Rail)
			{
				yOffset = -1;
				return true;
			}

			return false;
		}

		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			var up = position + BlockCoordinates.Up;

			string shape    = GetShape(state);
			var    hasNorth = Check(world, position, BlockFace.North, out var northernYOffset);
			var    hasEast  = Check(world, position, BlockFace.East, out var easternYOffset);
			var    hasSouth = Check(world, position, BlockFace.South, out var southernYOffset);
			var    hasWest = Check(world, position, BlockFace.West, out var westernYOffset);

			if (hasNorth && northernYOffset == 1)
			{
				shape = "ascending_north";
			}
			else if (hasEast && easternYOffset == 1)
			{
				shape = "ascending_east";
			}
			else if (hasSouth && southernYOffset == 1)
			{
				shape = "ascending_south";
			}
			else if (hasWest && westernYOffset == 1)
			{
				shape = "ascending_west";
			}
			
			if (hasNorth && hasEast)
			{
				shape = "south_east";
			}
			else if (hasNorth && hasWest)
			{
				//shape = "south_west";
				shape = "north_east";
			}
			else if (hasSouth && hasEast)
			{
				//shape = "north_east";
				shape = "south_west";
			}
			else if (hasSouth && hasWest)
			{
				shape = "north_west";
			}

			return state.WithProperty("shape", shape);

			/*if (Check(world, up, BlockFace.North))
			{
				shape = "ascending_north";
			}
			
			if (Check(world, up, BlockFace.East))
			{
				shape = "ascending_east";
			}
			
			if (Check(world, up, BlockFace.South))
			{
				shape = "ascending_south";
			}
			
			if (Check(world, up, BlockFace.West))
			{
				shape = "ascending_west";
			}*/
			//var north = world.GetBlockState(position + BlockCoordinates.North);
			//var east = world.GetBlockState(position + BlockCoordinates.East);
			//var south = world.GetBlockState(position + BlockCoordinates.South);
			//var west = world.GetBlockState(position + BlockCoordinates.West);

			/*if (UpdateState(world, state, position, position + BlockCoordinates.Forwards, out state)
			    || UpdateState(world, state, position, position + BlockCoordinates.Backwards, out state)
			    || UpdateState(world, state, position, position + BlockCoordinates.Left, out state)
			    || UpdateState(world, state, position, position + BlockCoordinates.Right, out state))
			{
				return state;
			}

			return state;*/
		}
	}
}
