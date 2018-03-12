using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using log4net;
using Microsoft.Xna.Framework;
using MiNET;
using MiNET.Entities;
using MiNET.Utils;

namespace Alex.Blocks
{
    public class Lever : Block
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Lever));
		public static PropertyBool Powered = new PropertyBool("powered");
		public Lever(uint blockStateId) : base(blockStateId)
	    {

	    }

		public override bool Tick(IWorld world, Vector3 position)
		{
			return base.Tick(world, position);
		}

		public override void Interact(IWorld world, BlockCoordinates position, BlockFace face, Entity sourcEntity)
		{
			var state = (BlockState) BlockState;
			world.SetBlockState(position.X, position.Y, position.Z, BlockFactory.GetBlockState((int)BlockFactory.GetBlockStateID(BlockId, (byte)(Metadata ^ 0x8))));
		/*	if (state.GetTypedValue(Powered))
			{
				world.SetBlockState(position.X, position.Y, position.Z, BlockFactory.GetBlockState((int) Block.GetBlockStateID(BlockId, (byte) (Metadata ^ 0x8))));
			}
			else
			{
				world.SetBlockState(position.X, position.Y, position.Z, (BlockState)state.WithProperty(Powered, true));
			}*/
		}
	}
}
