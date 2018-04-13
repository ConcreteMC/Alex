using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics
{
    public class TextureSlice2D
    {
        public Texture2D Texture { get; }

        public Rectangle Bounds { get; }

        public int Width => Bounds.Width;
        public int Height => Bounds.Height;


        public TextureSlice2D(Texture2D spriteSheet, Rectangle bounds)
        {
            Texture = spriteSheet;
            Bounds = bounds;
        }

        public static implicit operator TextureSlice2D(Texture2D texture)
        {
            return new TextureSlice2D(texture, texture.Bounds);
        }
    }
}
