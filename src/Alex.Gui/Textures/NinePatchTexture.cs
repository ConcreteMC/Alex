using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Textures
{
    public class NinePatchTexture
    {
        public int NineSliceSize { get; }

        public Texture2D Texture { get; }

        public NinePatchTexture(Texture2D texture, int sizeSlice = 0)
        {
            Texture = texture;
            NineSliceSize = sizeSlice;
        }

        public static explicit operator NinePatchTexture(Texture2D texture)
        {
            return new NinePatchTexture(texture);
        }
    }
}
