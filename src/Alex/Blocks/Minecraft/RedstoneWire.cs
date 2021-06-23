using System;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Graphics.Models.Blocks;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class PowerState : StateProperty<int>
	{
		public static readonly PowerState Instance = new PowerState();

		/// <inheritdoc />
		protected override StateProperty<int> WithValue(int value)
		{
			return new PowerState() {Value = value};
		}

		public override int ParseValue(string value)
		{
			return int.Parse(value);
		}

		/// <inheritdoc />
		public PowerState() : base("power") { }
	}

	public class RedstoneBase : Block
	{
		internal static readonly PropertyBool POWERED = new PropertyBool("powered");
		//public bool CanConnect()
		//{
			
		//}

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out StateProperty stateProperty)
		{
			switch (prop)
			{
				case "power":
					stateProperty = new PowerState();
					return true;
				case "powered":
					stateProperty = RedstoneBase.POWERED;
					return true;
			}
			return base.TryGetStateProperty(prop, out stateProperty);
		}
	}
	
	public class RedstoneWire : RedstoneBase, IMultipartCheck
	{
		public RedstoneWire() : base()
		{
			Solid = false;
			Transparent = true;
			RequiresUpdate = false;
			
			IsFullCube = false;
		}

		public override IMaterial BlockMaterial
		{
			get
			{
				var power = BlockState.GetTypedValue(PowerState.Instance);
				
				return Material.Circuits.Clone().SetTintType(TintType.Color, Colors[Math.Clamp(power, 0, Colors.Length - 1)]);
			}
		}

		private static readonly Color[] Colors = new Color[]
		{
			new Color(76, 0, 0), new Color(120, 2, 2), new Color(130, 5, 3), new Color(138, 8, 4),
			new Color(148, 12, 5), new Color(158, 16, 5), new Color(167, 19, 5), new Color(175, 23, 5),
			new Color(185, 26, 5), new Color(195, 30, 4), new Color(205, 33, 4), new Color(215, 36, 3),
			new Color(224, 40, 3), new Color(235, 43, 2), new Color(245, 46, 1), new Color(255, 50, 0)
		};

				
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is RedstoneWire)
				return true;

			return false;
		}
		
		private bool IsRedstoneWire(IBlockAccess world, Vector3 position)
		{
			var block = world.GetBlockState(position).Block;

			return block is RedstoneWire;
		}

		/// <inheritdoc />
		public bool Passes(IBlockAccess world, Vector3 position, string rule, string value)
		{
			if (!MultiPartModelHelper.TryGetBlockface(rule, out BlockFace face)) 
				return false;
			
			Vector3 offset = face.GetVector3();
			offset = position + offset;
			
			if (value == "side|up")
			{
				return IsRedstoneWire(world, offset)
				       || IsRedstoneWire(world, offset + Vector3.Down);
			}
			else if (value == "up")
			{
				return IsRedstoneWire(world, offset + Vector3.Up);
			}
			else if (value == "none")
			{
				return !IsRedstoneWire(world, offset);
			}
			
			return false;
		}

		/// <inheritdoc />
		public bool Passes(string rule, string value)
		{
			return SimplePass(rule, value);
		}
		
		private bool SimplePass(string rule, string value)
		{
			if (BlockState.TryGetValue(rule, out var val))
			{
				return val.Equals(value, StringComparison.InvariantCultureIgnoreCase);
			}

			return false;
		}

		private bool UpdateState(IBlockAccess world,
			BlockState state,
			BlockCoordinates position,
			BlockCoordinates updatedBlock,
			BlockFace face,
			out BlockState result)
		{
			result = state;

			return false;
			//	if (IsRedstoneWire(world, ))
		}

	/*	public override void BlockUpdate(World world, BlockCoordinates position, BlockCoordinates updatedBlock)
		{
			var newState = BlockState;

			if (Check(world, newState, position, BlockFace.North, out newState)
			    || Check(world, newState, position, BlockFace.East, out newState)
			    || Check(world, newState, position, BlockFace.South, out newState) || Check(
				    world, newState, position, BlockFace.West, out newState))
			{
				world.SetBlockState(position, newState);
				//return newState;
			}
			//	newState = Check(world, newState, position, BlockFace.East, out newState);
			//	newState = Check(world, newState, position, BlockFace.South, out newState);
			//newState = Check(world, newState, position, BlockFace.West, out newState);

			//return state;

			//return newState;
		}

		private bool Check(IBlockAccess world, BlockState state, BlockCoordinates position, BlockFace face, out BlockState result)
		{
			result = state;
			var stringified = face.ToString().ToLower();
			var offset = position + face.GetBlockCoordinates();

			bool isOffsetRedstone = IsRedstoneWire(world, offset);

			if (isOffsetRedstone || IsRedstoneWire(world, offset.BlockDown()))
			{
				result = state.WithProperty(stringified, "side|up");
				return true;
			}
			else if (IsRedstoneWire(world, offset.BlockUp()))
			{
				result = state.WithProperty(stringified.ToLower(), "up");
				return true;
			}
			else
			{
				result = state.WithProperty(stringified.ToLower(), "none");
				return true;
			}

			return false;
		}
		
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			var newState = state;//.VariantMapper.GetDefaultState();

			var changed = false;
			changed |= Check(world, newState, position, BlockFace.North, out newState);
			changed |= Check(world, newState, position, BlockFace.East, out newState);
			changed |= Check(world, newState, position, BlockFace.South, out newState);
			changed |= Check(world, newState, position, BlockFace.West, out newState);
			
			if (changed)
			{
				return newState;
			}
		//	newState = Check(world, newState, position, BlockFace.East, out newState);
		//	newState = Check(world, newState, position, BlockFace.South, out newState);
			//newState = Check(world, newState, position, BlockFace.West, out newState);

			return state;
			/*	if (UpdateState(world, state, position, position + BlockCoordinates.Forwards, BlockFace.South, out state)
				    || UpdateState(world, state, position, position + BlockCoordinates.Backwards, BlockFace.North, out state)
				    || UpdateState(world, state, position, position + BlockCoordinates.Left, BlockFace.West, out state)
				    || UpdateState(world, state, position, position + BlockCoordinates.Right, BlockFace.East, out state))
				{
					return state;
				}
	
				return state;*
		}*/
	}
}
