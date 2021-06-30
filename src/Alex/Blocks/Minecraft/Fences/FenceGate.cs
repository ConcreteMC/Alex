using Alex.Common.Blocks;

namespace Alex.Blocks.Minecraft.Fences
{
    public class FenceGate : Block
    {
        public FenceGate() : this(0)
        {

        }

        public FenceGate(uint id) : base()
        {
            Solid = true;
            Transparent = true;

            CanInteract = true;
            IsFullCube = false;

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