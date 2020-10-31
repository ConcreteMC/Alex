using Alex.API.Utils;
using Alex.Blocks.State;
using Alex.Entities.BlockEntities;
using Alex.Worlds;
using Alex.Worlds.Abstraction;

namespace Alex.Blocks.Minecraft
{
	public class EnderChest : Block
	{
		public EnderChest() : base()
		{
			Solid = true;
			Transparent = true;
			IsReplacible = false;
			LightValue = 7;
			
			Hardness = 22.5f;
			
			CanInteract = true;
			Renderable = false;

			HasHitbox = true;
		}
		
		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			if (world is World w)
			{
				if ((w.EntityManager.TryGetBlockEntity(position, out var entity) &&!(entity is EnderChestBlockEntity)))
				{
					w.EntityManager.RemoveBlockEntity(position);
				}
				
				var ent = new EnderChestBlockEntity(this, w, BlockEntityFactory.EnderChestTexture)
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
