using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics.Textures;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics.Typography
{
    public interface IFont
    {
        IReadOnlyCollection<char> Characters { get; }
        //IFontGlyph GetGlyphOrDefault(char character);

        Vector2 MeasureString(string text, float scale = 1.0f);
        Vector2 MeasureString(string text, Vector2 scale);
        void MeasureString(string text, out Vector2 size);

        void DrawString(SpriteBatch   sb, 
                        string text, 
                        Vector2 position,
                        TextColor     color,
                        FontStyle     style      = FontStyle.None, Vector2? scale = null,
                        float         opacity    = 1f,
                        float         rotation   = 0f, Vector2? origin = null,
                        SpriteEffects effects    = SpriteEffects.None,
                        float         layerDepth = 0f);
    }

    public interface IFontGlyph
    {
        char Character { get; }
        ITexture2D Texture { get; }
        float Width { get; }
        float Height { get; }
    }
}
