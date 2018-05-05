using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RocketUI
{
    public interface ITexture2D
    {
        Texture2D Texture { get; }
        Rectangle ClipBounds { get; }

        int Width { get; }
        int Height { get; }
    }
}
