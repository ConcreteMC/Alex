using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft.Fences
{
	public class Fence : Block
	{
		private static PropertyBool NORTH = new PropertyBool("north");
		private static PropertyBool EAST = new PropertyBool("east");
		private static PropertyBool SOUTH = new PropertyBool("south");
		private static PropertyBool WEST = new PropertyBool("west");
		
		public Fence()
		{
			Transparent = true;
			Solid = true;
			IsFullCube = false;
			RequiresUpdate = true;
		}

		public bool North => BlockState.GetTypedValue(NORTH);
		public bool East => BlockState.GetTypedValue(EAST);
		public bool South => BlockState.GetTypedValue(SOUTH);
		public bool West => BlockState.GetTypedValue(WEST);
		
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
	}
}