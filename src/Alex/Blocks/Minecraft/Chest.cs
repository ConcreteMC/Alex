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

			//RequiresUpdate = true;
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
				
				var ent = new ChestBlockEntity(this, w, BlockEntityFactory.ChestTexture)
				{
					X = position.X & 0xf, Y = position.Y & 0xff, Z = position.Z& 0xf
				};
					
				w.EntityManager.AddBlockEntity(
					position, ent);

				var chunk = world.GetChunk(position, true);

				if (chunk != null)
				{
					chunk.AddBlockEntity(new BlockCoordinates(ent.X, ent.Y, ent.Z), ent);
				}
			}
			return base.BlockPlaced(world, state, position);
		}
	}
}
