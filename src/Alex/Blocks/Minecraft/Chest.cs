using System.Collections.Generic;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Entities.BlockEntities;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class Chest : Block
	{
		public Chest() : base()
		{
			Solid = true;
			Transparent = true;

			CanInteract = true;
			Renderable = false;
			HasHitbox = true;

			RequiresUpdate = true;
		}

		/// <inheritdoc />
		public override IEnumerable<BoundingBox> GetBoundingBoxes(Vector3 blockPos)
		{
			yield return new BoundingBox(blockPos, blockPos + Vector3.One);
		}

		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			if (world is World w)
			{
				if ((w.EntityManager.TryGetBlockEntity(position, out var entity) && !(entity is ChestBlockEntity)))
				{
					w.EntityManager.RemoveBlockEntity(position);
				}

				if (entity is ChestBlockEntity)
					return base.BlockPlaced(world, state, position);

				var ent = new ChestBlockEntity(w) { X = position.X & 0xf, Y = position.Y & 0xff, Z = position.Z & 0xf };

				if (ent.SetBlock(this))
					w.SetBlockEntity(position.X, position.Y, position.Z, ent);
			}

			return base.BlockPlaced(world, state, position);
		}
	}
}