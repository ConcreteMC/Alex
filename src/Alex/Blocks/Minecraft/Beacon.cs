using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Entities.BlockEntities;
using Alex.Worlds;
using Alex.Worlds.Abstraction;

namespace Alex.Blocks.Minecraft
{
	public class Beacon : Block
	{
		public Beacon() : base()
		{
			Solid = true;
			Transparent = true;
			Luminance = 15;
			CanInteract = true;
			
			//Hardness = 3;

			BlockMaterial = Material.Glass.Clone().WithHardness(3f);
		}
		
		/*
		/// <inheritdoc />
		public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
		{
			if (world is World w)
			{
				if ((w.EntityManager.TryGetBlockEntity(position, out var entity) &&!(entity is BeaconBlockEntity)))
				{
					w.EntityManager.RemoveBlockEntity(position);
				}

				if (entity is BeaconBlockEntity) 
					return base.BlockPlaced(world, state, position);

				var ent = BlockEntityFactory.GetById("minecraft:beacon", w, this, position);
			}
			return base.BlockPlaced(world, state, position);
		}*/
	}
}
