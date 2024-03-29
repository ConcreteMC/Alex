﻿using Alex.Entities.BlockEntities;

namespace Alex.Blocks.Minecraft;

public class WallBanner : Block
{
	public BlockColor Color { get; set; }

	public WallBanner(BlockColor color)
	{
		Color = color;
		Solid = false;
		Transparent = true;
		Renderable = false;
		CanInteract = true;

		HasHitbox = true;
	}

	/*/// <inheritdoc />
	public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
	{
	    if (world is World w)
	    {
	        if ((w.EntityManager.TryGetBlockEntity(position, out var entity) &&!(entity is BannerBlockEntity)))
	        {
	            w.EntityManager.RemoveBlockEntity(position);

	            return base.BlockPlaced(world, state, position);
	        }

	        if (entity is BannerBlockEntity bbe)
	        {
	            bbe.SetBlock(this);
	            return base.BlockPlaced(world, state, position);
	        }

	    //    BlockEntityFactory.GetById("minecraft:wall_banner", w, this, position);
	    }
	    return base.BlockPlaced(world, state, position);
	}*/
}

public class StandingBanner : Block
{
	public BlockColor Color { get; set; }

	public StandingBanner(BlockColor color)
	{
		Color = color;

		Solid = false;
		Transparent = true;
		Renderable = false;
		CanInteract = true;

		HasHitbox = true;
	}

	/*/// <inheritdoc />
	public override BlockState BlockPlaced(IBlockAccess world, BlockState state, BlockCoordinates position)
	{
	    if (world is World w)
	    {
	        if ((w.EntityManager.TryGetBlockEntity(position, out var entity) &&!(entity is BannerBlockEntity)))
	        {
	            w.EntityManager.RemoveBlockEntity(position);
	            return base.BlockPlaced(world, state, position);
	        }

	        if (entity is BannerBlockEntity bbe)
	        {
	            bbe.SetBlock(this);
	            return base.BlockPlaced(world, state, position);
	        }

	        //BlockEntityFactory.GetById("minecraft:banner", w, this, position);
	    }
	    return base.BlockPlaced(world, state, position);
	}*/
}