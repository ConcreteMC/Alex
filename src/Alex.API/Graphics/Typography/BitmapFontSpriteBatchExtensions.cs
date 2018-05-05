using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;

namespace Alex.API.Graphics
{
    public static class BitmapFontSpriteBatchExtensions
	{

		public static void DrawString(this SpriteBatch sb,
		                              Vector2          position,
		                              string           text,
		                              IFont            font,
		                              TextColor        color,
		                              FontStyle        style      = FontStyle.None,
		                              float            rotation   = 0f,
		                              Vector2?         origin     = null,
		                              Vector2?         scale      = null,
		                              float            opacity    = 1f,
		                              SpriteEffects    effects    = SpriteEffects.None,
		                              float            layerDepth = 0f)
		{
			font.DrawString(sb, text, position, color.ForegroundColor, color.BackgroundColor, style, scale: scale, rotation: rotation, origin: origin, opacity: opacity, effects: effects, layerDepth: layerDepth);
		}
	}
}
