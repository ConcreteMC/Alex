using Alex.Blocks.Materials;
using Alex.Blocks.State;
using Alex.Common.Utils.Vectors;
using Alex.Entities.BlockEntities;
using Alex.Worlds;
using Alex.Worlds.Abstraction;

namespace Alex.Blocks.Minecraft;

public class WallBanner : Block
{
    public WallBanner(BlockColor color)
    {
        Solid = false;
        Transparent = true;
        Renderable = false;
        CanInteract = true;
			
        HasHitbox = true;

        BlockMaterial = Material.Wool.Clone().WithMapColor(color.ToMapColor());

       // RequiresUpdate = true;
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
    public StandingBanner(BlockColor color)
    {
        Solid = false;
        Transparent = true;
        Renderable = false;
        CanInteract = true;
			
        HasHitbox = true;
        
        BlockMaterial = Material.Wool.Clone().WithMapColor(color.ToMapColor());
       // RequiresUpdate = true;
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