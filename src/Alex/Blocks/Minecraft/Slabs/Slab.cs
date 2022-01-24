using Alex.Blocks.Properties;
using Alex.Blocks.State;
using Alex.Common.Blocks;
using Alex.Common.Blocks.Properties;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.ResourcePackLib.Json.Models.Items;
using Alex.Worlds;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft.Slabs
{
	public enum SlabType
	{
		Bottom,
		Top,
		Double
	}

	public class Slab : Block
	{
		public Slab() : base()
		{
			Solid = true;
			Transparent = true;
		}

		/// <inheritdoc />
		public override bool IsFullCube
		{
			get
			{
				if (BlockState.TryGetValue("type", out string t))
				{
					if (t == "double")
						return true;
				}

				return false;
			}
			set { }
		}

		private static readonly PropertyEnum<SlabType> TypeProp = new PropertyEnum<SlabType>("type", SlabType.Bottom);

		/// <inheritdoc />
		public override bool TryGetStateProperty(string prop, out IStateProperty stateProperty)
		{
			switch (prop)
			{
				case "type":
					stateProperty = TypeProp;

					return true;
			}

			return base.TryGetStateProperty(prop, out stateProperty);
		}

		/// <inheritdoc />
		public override bool PlaceBlock(World world,
			Player player,
			BlockCoordinates position,
			BlockFace face,
			Vector3 cursorPosition)
		{
			var targetBlock = world.GetBlockState(position);

			bool isSameBlockType = targetBlock.Block.GetType() == GetType();

			//if (face == BlockFace.Up || face == BlockFace.Down)
			//{
			if (isSameBlockType)
			{
				var targetType = targetBlock.GetValue(TypeProp);

				if (targetType != SlabType.Double)
				{
					if (targetType == SlabType.Bottom && face == BlockFace.Up)
					{
						world.SetBlockState(position, BlockState.WithProperty(TypeProp, SlabType.Double));

						return true;
					}
					else if (targetType == SlabType.Top && face == BlockFace.Down)
					{
						world.SetBlockState(position, BlockState.WithProperty(TypeProp, SlabType.Double));

						return true;
					}

					if ((targetType == SlabType.Bottom && cursorPosition.Y >= 0.5 && face != BlockFace.Down)
					    || (targetType == SlabType.Top && cursorPosition.Y <= 0.5 && face != BlockFace.Up))
					{
						world.SetBlockState(position, BlockState.WithProperty(TypeProp, SlabType.Double));

						return true;
					}
				}
			}
			// }

			// var targetBlock = world.GetBlockState(position);
			BlockState state = BlockState;

			if ((face != BlockFace.Up && cursorPosition.Y > 0.5) || (face == BlockFace.Down && cursorPosition.Y <= 0.5))
			{
				state = BlockState.WithProperty(TypeProp, SlabType.Top);
			}

			if (!targetBlock.Block.BlockMaterial.IsReplaceable)
			{
				position += face.GetBlockCoordinates();
			}

			world.SetBlockState(position, state);

			return true;
		}
	}
}