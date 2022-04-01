using Alex.Blocks.Materials;
using Alex.Blocks.Properties;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Entities.BlockEntities;
using Alex.Interfaces;

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

			if (block.Solid && !block.BlockMaterial.IsOpaque)
				return true;

			if (block.Solid && block.IsFullCube)
				return true;

			//if (block.IsFullCube)
			//	return true;

			return false;
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

		/// <inheritdoc />
		public override bool ShouldRenderFace(BlockFace face, Block neighbor)
		{
			bool connected = false;
			bool neighborConnected = false;

			switch (face)
			{
				case BlockFace.East:
					connected = BlockState.GetValue(PropertyBool.EAST);
					neighborConnected = neighbor.BlockState.GetValue(PropertyBool.WEST);

					break;

				case BlockFace.West:
					connected = BlockState.GetValue(PropertyBool.WEST);
					neighborConnected = neighbor.BlockState.GetValue(PropertyBool.EAST);

					break;

				case BlockFace.North:
					connected = BlockState.GetValue(PropertyBool.NORTH);
					neighborConnected = neighbor.BlockState.GetValue(PropertyBool.SOUTH);

					break;

				case BlockFace.South:
					connected = BlockState.GetValue(PropertyBool.SOUTH);
					neighborConnected = neighbor.BlockState.GetValue(PropertyBool.NORTH);

					break;
			}

			if (neighbor is GlassPane pane)
			{
				if (connected)
					return false;
			}

			return true;
		}
	}
}