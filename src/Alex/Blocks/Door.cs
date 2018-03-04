using Alex.API.World;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.ResourcePackLib.Json;
using log4net;
using Microsoft.Xna.Framework;

namespace Alex.Blocks
{
	public class Door : Block
	{
		private static ILog Log = LogManager.GetLogger(typeof(Door));

		public static PropertyBool OPEN = new PropertyBool("open");
		public static PropertyBool POWERED = new PropertyBool("powered");
		public static PropertyFace FACING = new PropertyFace("facing");

		public bool IsUpper => (Metadata & 0x08) == 0x08;
		public bool IsOpen => (Metadata & 0x04) == 0x04;
		public bool IsRightHinch => (Metadata & 0x01) == 0x01;
		public bool IsPowered => (Metadata & 0x02) == 0x02;

		public Door(int blockId, byte metadata) : base(blockId, metadata)
		{
			Transparent = true;
		}

		public override bool BlockUpdate(IWorld world, Vector3 position)
		{
			if (IsUpper)
			{
				Block below = (Block) world.GetBlock(position - new Vector3(0, 1, 0));
				if (below is Door bottom)
				{
					if (bottom.IsOpen)
					{
						
					}
				}
			}
			else
			{
				Block up = (Block)world.GetBlock(position + new Vector3(0, 1, 0));
				if (up is Door upper)
				{
					if (upper.IsRightHinch)
					{
				
					}
				}
			}
			return false;
		}
	}
}
