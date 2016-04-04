using Alex.Graphics.Items;

namespace Alex.Blocks
{
    public class Stone : Block
    {
        public Stone() : base(1, 0)
        {
            SetTexture(Utils.TextureSide.All, "stone");
        }
    }
}
