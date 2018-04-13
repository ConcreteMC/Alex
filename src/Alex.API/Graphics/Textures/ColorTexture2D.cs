using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics.Textures
{
    public class ColorTexture2D : TextureSlice2D
    {
        private static readonly Rectangle DefaultBounds = new Rectangle(0, 0, 1, 1);

        public ColorTexture2D(GraphicsDevice graphics, Color color) : base(CreateTexture(graphics, color), DefaultBounds)
        {
        }

        private static Texture2D CreateTexture(GraphicsDevice graphics, Color color)
        {
            var bounds = DefaultBounds;
            var texture = new Texture2D(graphics, bounds.Width, bounds.Height, false, SurfaceFormat.Color);
            var data = new Color[bounds.Width * bounds.Height];

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            texture.SetData(data);
            return texture;
        }
    }
}
