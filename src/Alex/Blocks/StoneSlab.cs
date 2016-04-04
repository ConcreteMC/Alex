using Alex.Utils;

namespace Alex.Blocks
{
    public class StoneSlab : Slab
    {
        public StoneSlab() : base(0)
        {
            SetTexture(TextureSide.Side, "stone_slab_side");
            SetTexture(TextureSide.Top, "stone_slab_top");
            SetTexture(TextureSide.Bottom, "stone_slab_top");
	        Solid = false;
        }
    }
}
