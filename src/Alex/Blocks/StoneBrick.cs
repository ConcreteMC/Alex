namespace Alex.Blocks
{
    public class StoneBrick : Block
    {
        public StoneBrick() : base(98, 0)
        {
            SetTexture(Utils.TextureSide.All, "stonebrick");
        }
    }
}
