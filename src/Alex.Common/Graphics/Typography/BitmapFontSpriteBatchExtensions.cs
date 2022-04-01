using Alex.Common.Utils;
using Alex.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.Common.Graphics.Typography
{
	// @formatter:off
    public static class BitmapFontSpriteBatchExtensions
	{
		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style = FontStyle.None)
		{
			DrawString(sb, font, text, position, color, style, 0f, Vector2.Zero, Vector2.One);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, float scale, TextColor color, FontStyle style = FontStyle.None)
		{
			DrawString(sb, font, text, position, color, style, 0f, Vector2.Zero, new Vector2(scale));
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, Vector2 scale, TextColor color, FontStyle style)
		{
			DrawString(sb, font, text, position, color, style, 0f, Vector2.Zero, scale);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, float scale, TextColor color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, font, text, position, color, style, rotation, origin.HasValue ? origin.Value : Vector2.Zero, new Vector2(scale));
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, Vector2 scale, TextColor color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, font, text, position, color, style, rotation, origin.HasValue ? origin.Value : Vector2.Zero, scale);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, font, text, position, color, style, rotation, origin.HasValue ? origin.Value : Vector2.Zero, Vector2.One);
		}

		public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style, float rotation, Vector2 origin, Vector2 scale, float opacity = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
		{
			font.DrawString(sb, text, position, (Color)color, style, scale: scale, rotation: rotation, origin: origin, opacity: opacity, effects: effects, layerDepth: layerDepth);
		}

		#region GuiSpriteBatch Shortcut Methods

		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style = FontStyle.None) =>
			DrawString(sb.SpriteBatch, font, text, position, color, style);
		
		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, float scale, TextColor color, FontStyle style = FontStyle.None) =>
			DrawString(sb.SpriteBatch, font, text, position, scale, color, style);
		
		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, Vector2 scale, TextColor color, FontStyle style) =>
			DrawString(sb.SpriteBatch, font, text, position, scale, color, style);
		
		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, float scale, TextColor color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null) =>
			DrawString(sb.SpriteBatch, font, text, position, scale, color, style, rotation, origin);
		
		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, Vector2 scale, TextColor color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null) =>
			DrawString(sb.SpriteBatch, font, text, position, scale, color, style, rotation, origin);
		
		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style = FontStyle.None, float rotation = 0f, Vector2? origin = null) =>
			DrawString(sb.SpriteBatch, font, text, position, color, style, rotation, origin);
		
		public static void DrawString(this GuiSpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style, float rotation, Vector2 origin, Vector2 scale, float opacity = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f) =>
			DrawString(sb.SpriteBatch, font, text, position, color, style, rotation, origin, scale, opacity, effects, layerDepth);
		
		#endregion
	}
	// @formatter:on
}