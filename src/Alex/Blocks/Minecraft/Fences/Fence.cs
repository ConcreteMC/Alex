using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft.Fences
{
	public class Fence : Block
	{
		public Fence()
		{
			Transparent = true;
			Solid = true;
			IsFullCube = false;
			RequiresUpdate = true;
		}

		public bool North => PropertyBool.NORTH.GetValue(BlockState);
		public bool East =>PropertyBool.EAST.GetValue(BlockState);
		public bool South => PropertyBool.SOUTH.GetValue(BlockState);
		public bool West => PropertyBool.WEST.GetValue(BlockState);
		
		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{/*
			var current = BlockState;
			current = Check(world, position, position + BlockCoordinates.North, current);
			current = Check(world, position, position + BlockCoordinates.East, current);
			current = Check(world, position, position + BlockCoordinates.South, current);
			current = Check(world, position, position + BlockCoordinates.West, current);
			current = Check(world, position, position + BlockCoordinates.Up, current);
			current = Check(world, position, position + BlockCoordinates.Down, current);
			return current;*/
			return base.BlockPlaced(world, state, position);
		}

		/// <inheritdoc />
		public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			var state = Check(world, position, updatedBlock, BlockState);
			if (state != BlockState)
				world.SetBlockState(position, state);
			
			//base.BlockUpdate(world, position, updatedBlock);
		}

		private BlockState Check(IBlockAccess world, BlockCoordinates position, BlockCoordinates updatedBlock, BlockState current)
		{
			var neighbor = world.GetBlockState(updatedBlock);
			
			var facePos = updatedBlock - position;
			var fp      = new Vector3(facePos.X, facePos.Y, facePos.Z);
			fp.Normalize();
			
			var face       = new Vector3(fp.X, fp.Y, fp.Z).GetBlockFace();
			var faceString = face.ToString().ToLower();

			bool currentValue = false;

			switch (face)
			{
				
				case BlockFace.East:
					currentValue = East;
					break;

				case BlockFace.West:
					currentValue = West;
					break;

				case BlockFace.North:
					currentValue = North;
					break;

				case BlockFace.South:
					currentValue = South;
					break;
			}
			//current.TryGetValue(faceString, out var currentValue);
			
			if (CanAttach(face, neighbor.Block))
			{
				if (!currentValue)
				{
					return current.WithProperty(faceString, "true");
					//world.SetBlockState(position, state);
				}
			}
			else 
			{
				if (currentValue)
				{
					return current.WithProperty(faceString, "false");
					//world.SetBlockState(position, state);
				}
			}

			return current;
		}

		/// <inheritdoc />
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is Fence || block is FenceGate)
				return true;
			
			return base.CanAttach(face, block);
		}

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			switch (prop)
			{
				case "north":
					stateProperty = PropertyBool.NORTH;
					return true;
				case "east":
					stateProperty = PropertyBool.EAST;
					return true;
				case "south":
					stateProperty = PropertyBool.SOUTH;
					return true;
				case "west":
					stateProperty = PropertyBool.WEST;
					return true;
				case "up":
					stateProperty = PropertyBool.UP;
					return true;
				case "down":
					stateProperty = PropertyBool.DOWN;
					return true;
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}