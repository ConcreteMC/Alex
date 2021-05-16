using System.Collections.Generic;
using Alex.API.Utils;
using Alex.API.Utils.Vectors;
using Alex.Blocks.State;
using Alex.Entities.BlockEntities;
using Alex.Worlds;
using Alex.Worlds.Abstraction;
using Microsoft.Xna.Framework;

namespace Alex.Blocks.Minecraft
{
	public class EnderChest : Chest
	{
		public EnderChest() : base()
		{
			Solid = true;
			Transparent = true;
			LightValue = 7;
			
			//Hardness = 22.5f;
			
			CanInteract = true;
			Renderable = false;

			HasHitbox = true;

			RequiresUpdate = true;
			BlockMaterial = Material.Stone.Clone().SetHardness(22.5f);
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
				if ((w.EntityManager.TryGetBlockEntity(position, out var entity) &&!(entity is EnderChestBlockEntity)))
				{
					w.EntityManager.RemoveBlockEntity(position);
				}

				if (entity is EnderChestBlockEntity)
					return base.BlockPlaced(world, state, position);
				
				var ent = new EnderChestBlockEntity(this, w)
				{
					X = position.X & 0xf, Y = position.Y & 0xff, Z = position.Z& 0xf
				};
				
				w.SetBlockEntity(position.X, position.Y, position.Z, ent);
					
				/*w.EntityManager.AddBlockEntity(
					position, ent);

				var chunk = world.GetChunk(position, true);

				if (chunk != null)
				{
					chunk.AddBlockEntity(new BlockCoordinates(ent.X, ent.Y, ent.Z), ent);
				}*/
			}
			return base.BlockPlaced(world, state, position);
		}
	}
}
