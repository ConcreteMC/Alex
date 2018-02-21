namespace Alex.Blocks
{
    public class Dirt : Block
    {
        public Dirt() : base(3, 0)
        {
            SetTexture(Utils.TextureSide.All, "dirt");
        }
    }
}
