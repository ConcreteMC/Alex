using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RocketUI.Graphics.Textures
{
    public class TextureSlice2D : ITexture2D
    {
        public Texture2D Texture { get; }

        public Rectangle ClipBounds { get; }

        public int Width => ClipBounds.Width;
        public int Height => ClipBounds.Height;


        public TextureSlice2D(Texture2D spriteSheet, Rectangle clipBounds)
        {
            Texture = spriteSheet;
            ClipBounds = clipBounds;
        }
        
        public static implicit operator TextureSlice2D(Texture2D texture)
        {
            return new TextureSlice2D(texture, texture.Bounds);
        }
    }
}
