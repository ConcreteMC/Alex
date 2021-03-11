using System;
using Alex.API.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.API.Graphics.Textures
{
    public class ColorTexture2D : ITexture2D, IDisposable
    {
        private static readonly Size DefaultSize = new Size(1, 1);
        
        public Texture2D Texture    { get; }
        public Rectangle ClipBounds { get; }
    
        public int Width  { get; }
        public int Height { get; }

        public ColorTexture2D(GraphicsDevice graphics, Color color) : this(graphics, color, DefaultSize)
        {
        }

        public ColorTexture2D(GraphicsDevice graphics, Color color, Size size)
        {
            Width = size.Width;
            Height = size.Height;
            ClipBounds = new Rectangle(0, 0, Width, Height);
            Texture = CreateTexture(graphics, ClipBounds, color);
        }


        private Texture2D CreateTexture(GraphicsDevice graphics, Rectangle bounds, Color color)
        {
            var texture = RocketUI.GpuResourceManager.CreateTexture2D(bounds.Width, bounds.Height, false, SurfaceFormat.Color);
            var data    = new Color[bounds.Width * bounds.Height];

            for (var i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            texture.SetData(data);
            return texture;
        }
        
        
        public static implicit operator Texture2D(ColorTexture2D texture)
        {
            return texture.Texture;
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }
    }
}
