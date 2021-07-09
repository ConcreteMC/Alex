using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;

namespace Alex.Blocks.Minecraft
{
	public class GlassPane : Block
	{
		public GlassPane() : base()
		{
			Solid = true;
			Transparent = true;
			IsFullCube = false;

			BlockMaterial = Material.Glass;
		}
		
		public override bool CanAttach(BlockFace face, Block block)
		{
			if (block is GlassPane)
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
