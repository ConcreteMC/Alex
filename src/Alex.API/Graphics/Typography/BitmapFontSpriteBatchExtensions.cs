using System;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Graphics.Typography
{
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

		public static void DrawString(this SpriteBatch sb,       IFont     font, string text,
		                              Vector2          position, TextColor color,
		                              FontStyle        style,    float     rotation, Vector2 origin,
		                              Vector2          scale,
		                              float            opacity = 1f,
		                              SpriteEffects    effects = SpriteEffects.None, float layerDepth = 0f)
		{
			font.DrawString(sb, text, position, color, style, scale: scale, rotation: rotation, origin: origin, opacity: opacity, effects: effects, layerDepth: layerDepth);
		}
		private static Random _fontRandomGenerator = new Random();
		//public static void DrawString(this SpriteBatch sb, IFont font, string text, Vector2 position, TextColor color, FontStyle style, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, float opacity = 1f)
		//{
		//	if (string.IsNullOrEmpty(text)) return;

		//	origin *= scale;

		//	var flipAdjustment = Vector2.Zero;

		//	var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
		//	var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			
		//	if (flippedVert || flippedHorz)
		//	{
		//		Vector2 size;
                
		//		bitmapFont.MeasureString(text, out size);

		//		if (flippedHorz)
		//		{
		//			origin.X         *= -1;
		//			flipAdjustment.X =  -size.X;
		//		}

		//		if (flippedVert)
		//		{
		//			origin.Y         *= -1;
		//			flipAdjustment.Y =  bitmapFont.LineSpacing - size.Y;
		//		}
		//	}

		//	Matrix transformation = Matrix.Identity;
		//	float  cos            = 0, sin = 0;
		//	if (rotation == 0)
		//	{
		//		transformation.M11 = (flippedHorz ? -scale.X : scale.X);
		//		transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
		//		transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
		//		transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
		//	}
		//	else
		//	{
		//		cos                = (float)Math.Cos(rotation);
		//		sin                = (float)Math.Sin(rotation);
		//		transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
		//		transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
		//		transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
		//		transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
		//		transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
		//		transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y; 
		//	}

		//	var offset           = Vector2.Zero;
		//	var firstGlyphOfLine = true;

		//	TextColor styleColor = color;
		//	bool styleRandom = false, styleBold = false, styleItalic = false, styleUnderline = false, styleStrikethrough = false;

		//	var blendFactor = sb.GraphicsDevice.BlendFactor;
		//	sb.GraphicsDevice.BlendFactor = Color.White * opacity;

		//	for (int i = 0; i < text.Length; i++)
		//	{
		//		char c = text[i];

		//		if(c == '\r') continue;
				
		//		if (c == '\n')
		//		{
		//			offset.X =  0.0f;
		//			offset.Y += bitmapFont.LineSpacing;

		//			firstGlyphOfLine = true;

		//			styleRandom        = false;
		//			styleBold          = false;
		//			styleStrikethrough = false;
		//			styleUnderline     = false;
		//			styleItalic        = false;
		//			styleColor         = color;
		//		}
		//		else if (c == '\x00A7')
		//		{
		//			// Formatting

		//			// Get next character
		//			if(i + 1 >= text.Length) continue;
					
		//			i++;
		//			var formatChar = text.ToLower()[i];
		//			if ("0123456789abcdef".IndexOf(formatChar) > 0)
		//			{
		//				styleColor = TextColor.GetColor(formatChar);
		//			}
		//			else if (formatChar == 'k')
		//			{
		//				styleRandom = true;
		//			}
		//			else if (formatChar == 'l')
		//			{
		//				styleBold = true;
		//			}
		//			else if (formatChar == 'm')
		//			{
		//				styleStrikethrough = true;
		//			}
		//			else if (formatChar == 'n')
		//			{
		//				styleUnderline = true;
		//			}
		//			else if (formatChar == 'o')
		//			{
		//				styleItalic = true;
		//			}
		//			else if (formatChar == 'r')
		//			{
		//				styleRandom = false;
		//				styleBold = false;
		//				styleStrikethrough = false;
		//				styleUnderline = false;
		//				styleItalic = false;
		//				styleColor = TextColor.White;
		//			}
		//		}
		//		else
		//		{
		//			var glyph = bitmapFont.GetGlyphOrDefault(c);

		//			if (firstGlyphOfLine)
		//			{
		//				offset.X += bitmapFont.CharacterSpacing;
		//				firstGlyphOfLine = false;
		//			}

		//			//if (styleRandom)
		//			//{
		//			//	c = 
		//			//}

		//			var p = offset;

		//			if (dropShadow)
		//			{
		//				var shadowP = p + Vector2.One;
						
		//				if (styleBold)
		//				{
		//					var boldShadowP = Vector2.Transform(shadowP + Vector2.UnitX, transformation);
		//					sb.Draw(glyph.Texture, boldShadowP, styleColor.BackgroundColor * opacity, rotation, origin, scale, effects, layerDepth);
		//				}
						
		//				shadowP = Vector2.Transform(shadowP, transformation);

		//				sb.Draw(glyph.Texture, shadowP, styleColor.BackgroundColor * opacity, rotation, origin, scale, effects, layerDepth);
		//			}

		//			if (styleBold)
		//			{
		//				var boldP = Vector2.Transform(p + Vector2.UnitX, transformation);
		//				sb.Draw(glyph.Texture, boldP, styleColor.ForegroundColor * opacity, rotation, origin, scale, effects, layerDepth);
		//			}

		//			p = Vector2.Transform(p, transformation);

		//			sb.Draw(glyph.Texture, p, styleColor.ForegroundColor * opacity, rotation, origin, scale, effects, layerDepth);

		//			offset.X += glyph.Width + (styleBold ? 1 : 0) + bitmapFont.CharacterSpacing;
		//		}
		//	}

		//	sb.GraphicsDevice.BlendFactor = blendFactor;
		//}
	}
}
