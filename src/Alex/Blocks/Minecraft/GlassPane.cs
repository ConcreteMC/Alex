using Alex.Blocks.Materials;
using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft
{
	public class StainedGlassPane : GlassPane
	{
		public readonly BlockColor Color;
		public StainedGlassPane(BlockColor color)
		{
			Color = color;
		}
	}
	public class GlassPane : Block
	{
		public GlassPane() : base()
		{
			Solid = true;
			Transparent = true;
			base.IsFullCube = false;

			base.BlockMaterial = Material.Glass;
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
