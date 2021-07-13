using Alex.Blocks.Materials;
using Alex.Blocks.Minecraft.Slabs;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class Vine : Block
	{
		public bool North => PropertyBool.NORTH.GetValue(BlockState);
		public bool East =>PropertyBool.EAST.GetValue(BlockState);
		public bool South => PropertyBool.SOUTH.GetValue(BlockState);
		public bool West => PropertyBool.WEST.GetValue(BlockState);
		public bool Up => PropertyBool.UP.GetValue(BlockState);
		
		public Vine() : base()
		{
			Solid = false;
			Transparent = true;
			RequiresUpdate = true;
			BlockMaterial = Material.Vine;
			IsFullCube = false;
		}
		
		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			state = Check(world, position, position + BlockCoordinates.North, state);
			state = Check(world, position, position + BlockCoordinates.South, state);
			state = Check(world, position, position + BlockCoordinates.East, state);
			state = Check(world, position, position + BlockCoordinates.West, state);
			state = Check(world, position, position + BlockCoordinates.Up, state);
			
			return state;
			
			//return base.BlockPlaced(world, state, position);
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
			PropertyBool prop = PropertyBool.UP;
			switch (face)
			{
				
				case BlockFace.East:
					currentValue = East;
					prop = PropertyBool.EAST;
					break;

				case BlockFace.West:
					currentValue = West;
					prop = PropertyBool.WEST;
					break;

				case BlockFace.North:
					currentValue = North;
					prop = PropertyBool.NORTH;
					break;

				case BlockFace.South:
					currentValue = South;
					prop = PropertyBool.SOUTH;
					break;
				
				case BlockFace.Up:
					currentValue = Up;
					prop = PropertyBool.UP;
					break;
			}
			//current.TryGetValue(faceString, out var currentValue);
			
			if (CanAttach(face, neighbor.Block))
			{
				if (!currentValue)
				{
					return current.WithProperty(prop, true);
					//world.SetBlockState(position, state);
				}
			}
			else 
			{
				if (face != BlockFace.Up)
				{
					var up = world.GetBlockState(position + BlockCoordinates.Up);

					if (up.Block is Vine vine)
					{
						if (prop.GetValue(vine.BlockState))
						{
							return current.WithProperty(prop, true);
						}
					}
				}

				if (currentValue)
				{
					return current.WithProperty(prop, false);
					//world.SetBlockState(position, state);
				}
			}

			return current;
		}

		/// <inheritdoc />
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block.Solid && block.IsFullCube)
				return true;

			return base.CanAttach(face, block);
		}
		
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
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}
