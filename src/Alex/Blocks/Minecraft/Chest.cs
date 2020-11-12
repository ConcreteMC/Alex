using Alex.API.Utils;
using Alex.Blocks.State;
using Alex.Entities.BlockEntities;
using Alex.Worlds;
using Alex.Worlds.Abstraction;

namespace Alex.Blocks.Minecraft
{
	public class Chest : Block
	{
		public Chest() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;

			CanInteract = true;
			Renderable = false;
			HasHitbox = true;

			RequiresUpdate = true;
		}

		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			if (world is World w)
			{
				if ((w.EntityManager.TryGetBlockEntity(position, out var entity) &&!(entity is ChestBlockEntity)))
				{
					w.EntityManager.RemoveBlockEntity(position);
				}

				if (entity is ChestBlockEntity) 
					return base.BlockPlaced(world, state, position);

				var ent = new ChestBlockEntity(this, w, BlockEntityFactory.ChestTexture)
				{
					X = position.X & 0xf, Y = position.Y & 0xff, Z = position.Z & 0xf
				};

				w.SetBlockEntity(position.X, position.Y, position.Z, ent);
			}
			return base.BlockPlaced(world, state, position);
		}
	}
}
