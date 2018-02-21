using Alex.Utils;

namespace Alex.Blocks
{
    public class Bedrock : Block
    {
        public Bedrock() : base(7, 0)
        {
            SetTexture(TextureSide.All, "bedrock");
        }
    }
}
