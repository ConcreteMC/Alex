using Alex.Utils;

namespace Alex.Blocks
{
    public class StoneBrickSlab : Slab
    {
        public StoneBrickSlab() : base(5)
        {
            SetTexture(TextureSide.Side, "stonebrick");
            SetTexture(TextureSide.Top, "stonebrick");
            SetTexture(TextureSide.Bottom, "stonebrick");
	       // Solid = false;
	        Transparent = true;
        }
    }
}
