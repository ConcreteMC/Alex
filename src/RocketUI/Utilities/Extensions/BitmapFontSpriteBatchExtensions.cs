using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RocketUI.Utilities
{
    public static class BitmapFontSpriteBatchExtensions
	{
		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, Color color, FontStyle style = FontStyle.None)
		{
			DrawString(sb, font, text, position, color, style, 0f, Vector2.Zero, Vector2.One);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, float scale, Color color, FontStyle style = FontStyle.None)
		{
			DrawString(sb, font, text, position, color, style, 0f, Vector2.Zero, new Vector2(scale));
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, Vector2 scale, Color color, FontStyle style)
		{
			DrawString(sb, font, text, position, color, style, 0f, Vector2.Zero, scale);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, float scale, Color color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, font, text, position, color, style, rotation, origin.HasValue ? origin.Value : Vector2.Zero, new Vector2(scale));
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, Vector2 scale, Color color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, font, text, position, color, style, rotation, origin.HasValue ? origin.Value : Vector2.Zero, scale);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, Color color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, font, text, position, color, style, rotation, origin.HasValue ? origin.Value : Vector2.Zero, Vector2.One);
		}

		public static void DrawString(this SpriteBatch sb,       IFont font,     string  text,
		                              Vector2          position, Color color,
		                              FontStyle        style,    float rotation, Vector2 origin,
		                              Vector2          scale,
		                              float            opacity = 1f,
		                              SpriteEffects    effects = SpriteEffects.None, float layerDepth = 0f)
		{
			font.DrawString(sb, text, position, color, Color.TransparentBlack, style, scale: scale, rotation: rotation, origin: origin, opacity: opacity, effects: effects, layerDepth: layerDepth);
		}

		public static void DrawString(this SpriteBatch sb,       IFont     font, string text,
		                              Vector2          position, Color color, Color shadowColor,
		                              FontStyle        style,    float     rotation, Vector2 origin,
		                              Vector2          scale,
		                              float            opacity = 1f,
		                              SpriteEffects    effects = SpriteEffects.None, float layerDepth = 0f)
		{
			font.DrawString(sb, text, position, color, shadowColor, style, scale: scale, rotation: rotation, origin: origin, opacity: opacity, effects: effects, layerDepth: layerDepth);
		}
	}
}
