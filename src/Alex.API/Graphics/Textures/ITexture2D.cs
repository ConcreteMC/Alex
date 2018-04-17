using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics.Textures
{
    public interface ITexture2D
    {
        Texture2D Texture { get; }
        Rectangle ClipBounds { get; }

        int Width { get; }
        int Height { get; }
    }
}
