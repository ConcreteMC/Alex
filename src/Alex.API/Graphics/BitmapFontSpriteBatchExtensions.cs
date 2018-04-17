using System;
using Alex.API.Graphics.Textures;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Alex.API.Graphics
{
    public static class BitmapFontSpriteBatchExtensions
	{
		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow = true)
		{
			DrawString(sb, bitmapFont, text, position, color, dropShadow, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, float scale, TextColor color, bool dropShadow = true)
		{
			DrawString(sb, bitmapFont, text, position, color, dropShadow, 0f, Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0f);
		}

		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, Vector2 scale, TextColor color, bool dropShadow = true)
		{
			DrawString(sb, bitmapFont, text, position, color, dropShadow, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, float scale, TextColor color, bool dropShadow = true, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, bitmapFont, text, position, color, dropShadow, rotation, origin.HasValue ? origin.Value : Vector2.Zero, new Vector2(scale), SpriteEffects.None, 0f);
		}

		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, Vector2 scale, TextColor color, bool dropShadow = true, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, bitmapFont, text, position, color, dropShadow, rotation, origin.HasValue ? origin.Value : Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow = true, float rotation = 0f, Vector2? origin = null)
		{
			DrawString(sb, bitmapFont, text, position, color, dropShadow, rotation, origin.HasValue ? origin.Value : Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public static void DrawString(this SpriteBatch sb, BitmapFont bitmapFont, string text, Vector2 position, TextColor color, bool dropShadow, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f, float opacity = 1f)
		{
			if (string.IsNullOrEmpty(text)) return;

			origin *= scale;

			var flipAdjustment = Vector2.Zero;

			var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			
			if (flippedVert || flippedHorz)
			{
				Vector2 size;
                
				bitmapFont.MeasureString(text, out size);

				if (flippedHorz)
				{
					origin.X         *= -1;
					flipAdjustment.X =  -size.X;
				}

				if (flippedVert)
				{
					origin.Y         *= -1;
					flipAdjustment.Y =  bitmapFont.LineSpacing - size.Y;
				}
			}

			Matrix transformation = Matrix.Identity;
			float  cos            = 0, sin = 0;
			if (rotation == 0)
			{
				transformation.M11 = (flippedHorz ? -scale.X : scale.X);
				transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
				transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
				transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
			}
			else
			{
				cos                = (float)Math.Cos(rotation);
				sin                = (float)Math.Sin(rotation);
				transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
				transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
				transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
				transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
				transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
				transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y; 
			}

			var offset           = Vector2.Zero;
			var firstGlyphOfLine = true;

			TextColor styleColor = color;
			bool styleRandom = false, styleBold = false, styleItalic = false, styleUnderline = false, styleStrikethrough = false;

			var blendFactor = sb.GraphicsDevice.BlendFactor;
			sb.GraphicsDevice.BlendFactor = Color.White * opacity;

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if(c == '\r') continue;
				
				if (c == '\n')
				{
					offset.X =  0.0f;
					offset.Y += bitmapFont.LineSpacing;
					firstGlyphOfLine = true;

					styleRandom        = false;
					styleBold          = false;
					styleStrikethrough = false;
					styleUnderline     = false;
					styleItalic        = false;
					styleColor         = color;
				}
				else if (c == '\x00A7')
				{
					// Formatting

					// Get next character
					if(i + 1 >= text.Length) continue;
					
					i++;
					var formatChar = text.ToLower()[i];
					if ("0123456789abcdef".IndexOf(formatChar) > 0)
					{
						styleColor = TextColor.GetColor(formatChar);
					}
					else if (formatChar == 'k')
					{
						styleRandom = true;
					}
					else if (formatChar == 'l')
					{
						styleBold = true;
					}
					else if (formatChar == 'm')
					{
						styleStrikethrough = true;
					}
					else if (formatChar == 'n')
					{
						styleUnderline = true;
					}
					else if (formatChar == 'o')
					{
						styleItalic = true;
					}
					else if (formatChar == 'r')
					{
						styleRandom = false;
						styleBold = false;
						styleStrikethrough = false;
						styleUnderline = false;
						styleItalic = false;
						styleColor = TextColor.White;
					}
				}
				else
				{
					var glyph = bitmapFont.GetGlyphOrDefault(c);

					if (firstGlyphOfLine)
					{
						offset.X += bitmapFont.CharacterSpacing;
					}

					firstGlyphOfLine = false;

					var p = offset;

					if (dropShadow)
					{
						var shadowP = p + Vector2.One;
						
						if (styleBold)
						{
							var boldShadowP = Vector2.Transform(shadowP + Vector2.UnitX, transformation);
							sb.Draw(glyph.TextureSlice, boldShadowP, styleColor.BackgroundColor, rotation, origin, scale, effects, layerDepth);
						}
						
						shadowP = Vector2.Transform(shadowP, transformation);

						sb.Draw(glyph.TextureSlice, shadowP, styleColor.BackgroundColor, rotation, origin, scale, effects, layerDepth);
					}

					if (styleBold)
					{
						var boldP = Vector2.Transform(p + Vector2.UnitX, transformation);
						sb.Draw(glyph.TextureSlice, boldP, glyph.TextureSlice.ClipBounds, styleColor.ForegroundColor, rotation, origin, scale, effects, layerDepth);
					}

					p = Vector2.Transform(p, transformation);

					sb.Draw(glyph.TextureSlice, p, styleColor.ForegroundColor, rotation, origin, scale, effects, layerDepth);

					offset.X += glyph.Width + bitmapFont.CharacterSpacing;
				}
			}

			sb.GraphicsDevice.BlendFactor = blendFactor;
		}
	}

	
  internal class SpriteBatchItem : IComparable<SpriteBatchItem>
  {
    public Texture2D Texture;
    public float SortKey;
    public VertexPositionColorTexture vertexTL;
    public VertexPositionColorTexture vertexTR;
    public VertexPositionColorTexture vertexBL;
    public VertexPositionColorTexture vertexBR;

    public SpriteBatchItem()
    {
      this.vertexTL = new VertexPositionColorTexture();
      this.vertexTR = new VertexPositionColorTexture();
      this.vertexBL = new VertexPositionColorTexture();
      this.vertexBR = new VertexPositionColorTexture();
    }

    public void Set(float x, float y, float dx, float dy, float w, float h, float sin, float cos, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
    {
      this.vertexTL.Position.X = (float) ((double) x + (double) dx * (double) cos - (double) dy * (double) sin);
      this.vertexTL.Position.Y = (float) ((double) y + (double) dx * (double) sin + (double) dy * (double) cos);
      this.vertexTL.Position.Z = depth;
      this.vertexTL.Color = color;
      this.vertexTL.TextureCoordinate.X = texCoordTL.X;
      this.vertexTL.TextureCoordinate.Y = texCoordTL.Y;
      this.vertexTR.Position.X = (float) ((double) x + ((double) dx + (double) w) * (double) cos - (double) dy * (double) sin);
      this.vertexTR.Position.Y = (float) ((double) y + ((double) dx + (double) w) * (double) sin + (double) dy * (double) cos);
      this.vertexTR.Position.Z = depth;
      this.vertexTR.Color = color;
      this.vertexTR.TextureCoordinate.X = texCoordBR.X;
      this.vertexTR.TextureCoordinate.Y = texCoordTL.Y;
      this.vertexBL.Position.X = (float) ((double) x + (double) dx * (double) cos - ((double) dy + (double) h) * (double) sin);
      this.vertexBL.Position.Y = (float) ((double) y + (double) dx * (double) sin + ((double) dy + (double) h) * (double) cos);
      this.vertexBL.Position.Z = depth;
      this.vertexBL.Color = color;
      this.vertexBL.TextureCoordinate.X = texCoordTL.X;
      this.vertexBL.TextureCoordinate.Y = texCoordBR.Y;
      this.vertexBR.Position.X = (float) ((double) x + ((double) dx + (double) w) * (double) cos - ((double) dy + (double) h) * (double) sin);
      this.vertexBR.Position.Y = (float) ((double) y + ((double) dx + (double) w) * (double) sin + ((double) dy + (double) h) * (double) cos);
      this.vertexBR.Position.Z = depth;
      this.vertexBR.Color = color;
      this.vertexBR.TextureCoordinate.X = texCoordBR.X;
      this.vertexBR.TextureCoordinate.Y = texCoordBR.Y;
    }

    public void Set(float x, float y, float w, float h, Color color, Vector2 texCoordTL, Vector2 texCoordBR, float depth)
    {
      this.vertexTL.Position.X = x;
      this.vertexTL.Position.Y = y;
      this.vertexTL.Position.Z = depth;
      this.vertexTL.Color = color;
      this.vertexTL.TextureCoordinate.X = texCoordTL.X;
      this.vertexTL.TextureCoordinate.Y = texCoordTL.Y;
      this.vertexTR.Position.X = x + w;
      this.vertexTR.Position.Y = y;
      this.vertexTR.Position.Z = depth;
      this.vertexTR.Color = color;
      this.vertexTR.TextureCoordinate.X = texCoordBR.X;
      this.vertexTR.TextureCoordinate.Y = texCoordTL.Y;
      this.vertexBL.Position.X = x;
      this.vertexBL.Position.Y = y + h;
      this.vertexBL.Position.Z = depth;
      this.vertexBL.Color = color;
      this.vertexBL.TextureCoordinate.X = texCoordTL.X;
      this.vertexBL.TextureCoordinate.Y = texCoordBR.Y;
      this.vertexBR.Position.X = x + w;
      this.vertexBR.Position.Y = y + h;
      this.vertexBR.Position.Z = depth;
      this.vertexBR.Color = color;
      this.vertexBR.TextureCoordinate.X = texCoordBR.X;
      this.vertexBR.TextureCoordinate.Y = texCoordBR.Y;
    }

    public int CompareTo(SpriteBatchItem other)
    {
      return this.SortKey.CompareTo(other.SortKey);
    }
  }
}
