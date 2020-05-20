using Alex.API.Blocks;
using Alex.API.World;

namespace Alex.Blocks.Minecraft
{
    public class FenceGate : Block
    {
        public FenceGate() : this(0)
        {

        }

        public FenceGate(uint id) : base(id)
        {
            Solid = true;
            Transparent = true;
            IsReplacible = false;

            CanInteract = true;
            IsFullCube = false;
            
            Hardness = 2;
            
            BlockMaterial = Material.Wood;
        }

        /// <inheritdoc />
        public override bool CanAttach(BlockFace face, Block block)
        {
            if (block is Fence || block is FenceGate)
                return true;
            
            return base.CanAttach(face, block);
        }
    }
}