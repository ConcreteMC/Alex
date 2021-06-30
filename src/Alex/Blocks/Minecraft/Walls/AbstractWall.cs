using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Utils.Vectors;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft.Walls
{
	public abstract class AbstractWall : Block
	{
		protected AbstractWall()
		{
			Solid = true;
			Transparent = true;
			
			IsFullCube = false;
			RequiresUpdate = true;

			BlockMaterial = Material.Stone;
		}

		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			state = Check(world, position, position + BlockCoordinates.North, state);
			state = Check(world, position, position + BlockCoordinates.South, state);
			state = Check(world, position, position + BlockCoordinates.East, state);
			state = Check(world, position, position + BlockCoordinates.West, state);

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
			string checkAttach = "low";
			var up = world.GetBlockState(position + BlockCoordinates.Up);

			if (CanAttach(BlockFace.Up, up.Block))
				checkAttach = "tall";
			
			var neighbor = world.GetBlockState(updatedBlock);
			
			var facePos = updatedBlock - position;
			var fp      = new Vector3(facePos.X, facePos.Y, facePos.Z);
			fp.Normalize();
			
			var face       = new Vector3(fp.X, fp.Y, fp.Z).GetBlockFace();
			var faceString = face.ToString().ToLower();
			
			current.TryGetValue(faceString, out var currentValue);
			
			if (CanAttach(face, neighbor.Block))
			{
				if (currentValue != checkAttach)
				{
					return current.WithProperty(faceString, checkAttach);
					//world.SetBlockState(position, state);
				}
			}
			else 
			{
				if (currentValue != "none")
				{
					return current.WithProperty(faceString, "none");
					//world.SetBlockState(position, state);
				}
			}

			return current;
		}

		/// <inheritdoc />
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is AbstractWall)
				return true;
			
			return base.CanAttach(face, block);
		}

		//private static readonly PropertyBool DirectionalProp = new PropertyBool("")
		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
		{
		/*	switch (prop)
			{
				case "up":
				case "north":
				case "east":
				case "south":
				case "west":
					stateProperty = new PropertyBool(prop, "true", "none");
					return true;
			}*/
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
}