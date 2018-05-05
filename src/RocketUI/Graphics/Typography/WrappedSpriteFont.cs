using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RocketUI.Graphics.Typography
{
    public class WrappedSpriteFont : IFont
    {
        public IReadOnlyCollection<char> Characters => _spriteFont.Characters;

        private readonly SpriteFont _spriteFont;

        public WrappedSpriteFont(SpriteFont spriteFont)
        {
            _spriteFont = spriteFont;
        }

        public Vector2 MeasureString(string text, float scale = 1)
        {
            return _spriteFont.MeasureString(text) * scale;
        }

        public Vector2 MeasureString(string text, Vector2 scale)
        {
            return _spriteFont.MeasureString(text) * scale;
        }

        public void MeasureString(string text, out Vector2 size)
        {
            size = _spriteFont.MeasureString(text);
        }

        public void DrawString(SpriteBatch   sb, string text, Vector2 position,
                               Color     color, Color? shadowColor = null,
                               FontStyle     style      = FontStyle.None, Vector2? scale = null,
                               float         opacity    = 1f,
                               float         rotation   = 0f, Vector2? origin = null,
                               SpriteEffects effects    = SpriteEffects.None,
                               float         layerDepth = 0f)
        {

            if (shadowColor.HasValue)
            {
                sb.DrawString(_spriteFont, text, position + Vector2.One, shadowColor.Value * opacity, rotation,
                              origin ?? Vector2.Zero, scale ?? Vector2.One, effects, layerDepth);
            }

            sb.DrawString(_spriteFont, text, position, color * opacity, rotation, origin ?? Vector2.Zero, scale ?? Vector2.One, effects, layerDepth);
        }
        
        public static implicit operator WrappedSpriteFont(SpriteFont spriteFont)
        {
            return new WrappedSpriteFont(spriteFont);
        }

        public static implicit operator SpriteFont(WrappedSpriteFont font)
        {
            return font._spriteFont;
        }
    }
}
