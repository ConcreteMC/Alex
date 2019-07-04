using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Alex.API.Graphics.Textures;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.API.Graphics.Typography
{
    public class BitmapFont : IFont
    {

        public IReadOnlyCollection<char> Characters { get; }

        public int GridWidth  { get; }
        public int GridHeight { get; }

        public int LineSpacing { get; set; } = 10;
        public int CharacterSpacing { get; set; } = 1;

        public Texture2D Texture { get; }

        public Glyph DefaultGlyph { get; private set; }
        public Glyph[] Glyphs { get; private set; }

        private List<char> _characters;

        private bool _isInitialised = false;

        public BitmapFont(GraphicsDevice graphics, Bitmap bitmap, int gridSize, List<char> characters) :
            this(graphics, bitmap, gridSize, gridSize, characters)
        {

        }
        public BitmapFont(GraphicsDevice graphics, Bitmap bitmap, int gridWidth, int gridHeight, List<char> characters) :
            this(TextureUtils.BitmapToTexture2D(graphics, bitmap), gridWidth, gridHeight, characters)
        {
            LoadGlyphs(bitmap, characters);
        }

        public BitmapFont(Texture2D texture, int gridSize, List<char> characters) : this(texture, gridSize, gridSize, characters)
        {

        }

        public BitmapFont(Texture2D texture, int gridWidth, int gridHeight, List<char> characters) : this(gridWidth, gridHeight, characters)
        {
            Texture = texture;
        }

        private BitmapFont(int gridWidth, int gridHeight, List<char> characters)
        {
            GridWidth   = gridWidth;
            GridHeight  = gridHeight;
            _characters = characters;
            Characters = _characters.ToArray();

            DefaultGlyph = new Glyph('\x0000', null, 0, 8);
            Glyphs       = new Glyph[characters.Count];
        }

        public Vector2 MeasureString(string text, float scale = 1.0f)
        {
            return MeasureString(text, new Vector2(scale));
        }

        public Vector2 MeasureString(string text, Vector2 scale)
        {
            MeasureString(text, out var size);
            return size * scale;
        }

        public void MeasureString(string text, out Vector2 size)
        {
            if (string.IsNullOrEmpty(text))
            {
                size = Vector2.Zero;
                return;
            }
            
            float width = 0.0f, finalLineHeight = LineSpacing;
            Vector2 offset = Vector2.Zero;
            var firstGlyphOfLine = true;
            
            bool styleRandom = false, styleBold = false, styleItalic = false, styleUnderline = false, styleStrikethrough = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if(c == '\r') continue;

                if (c == '\n')
                {
                    offset.X = 0.0f;
                    offset.Y += LineSpacing;

                    finalLineHeight = LineSpacing;
                    firstGlyphOfLine = true;

                    styleRandom        = false;
                    styleBold          = false;
                    styleStrikethrough = false;
                    styleUnderline     = false;
                    styleItalic        = false;
                }
                else if (c == '\x00A7')
                {
                    // Formatting
                    
                    // Get next character
                    if(i + 1 >= text.Length) continue;

                    i++;
                    var formatChar = text.ToLower()[i];
                    if (formatChar == 'k')
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
                        styleRandom        = false;
                        styleBold          = false;
                        styleStrikethrough = false;
                        styleUnderline     = false;
                        styleItalic        = false;
                    }
                }
                else
                {
                    var glyph = GetGlyphOrDefault(c);

                    if (firstGlyphOfLine)
                    {
                       // offset.X += CharacterSpacing;
	                    firstGlyphOfLine = false;
					}

                    offset.X += glyph.Width + (styleBold ? 1 : 0) + CharacterSpacing;

                    finalLineHeight = Math.Max(finalLineHeight, glyph.Height);
                    width           = Math.Max(width, offset.X);
                }
            }

            size.X = width;
            size.Y = offset.Y + finalLineHeight;
        }

        public void DrawString(SpriteBatch   sb, string text, Vector2 position,
                               TextColor     color,
                               FontStyle     style      = FontStyle.None, Vector2? scale = null,
                               float         opacity    = 1f,
                               float         rotation   = 0f, Vector2? origin = null,
                               SpriteEffects effects    = SpriteEffects.None,
                               float         layerDepth = 0f)
		{
			if (string.IsNullOrEmpty(text)) return;

			var originVal = origin ?? Vector2.Zero;
			var scaleVal = scale ?? Vector2.One;

			originVal *= scaleVal;

			var flipAdjustment = Vector2.Zero;

			var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
			var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;
			
			if (flippedVert || flippedHorz)
			{
				Vector2 size;
                
				MeasureString(text, out size);

				if (flippedHorz)
				{
					originVal.X         *= -1;
					flipAdjustment.X =  -size.X;
				}

				if (flippedVert)
				{
					originVal.Y         *= -1;
					flipAdjustment.Y =  LineSpacing - size.Y;
				}
			}

			Matrix transformation = Matrix.Identity;
			float  cos            = 0, sin = 0;
			if (rotation == 0)
			{
				transformation.M11 = (flippedHorz ? -scaleVal.X : scaleVal.X);
				transformation.M22 = (flippedVert ? -scaleVal.Y : scaleVal.Y);
				transformation.M41 = ((flipAdjustment.X - originVal.X) * transformation.M11) + position.X;
				transformation.M42 = ((flipAdjustment.Y - originVal.Y) * transformation.M22) + position.Y;
			}
			else
			{
				cos                = (float)Math.Cos(rotation);
				sin                = (float)Math.Sin(rotation);
				transformation.M11 = (flippedHorz ? -scaleVal.X : scaleVal.X) * cos;
				transformation.M12 = (flippedHorz ? -scaleVal.X : scaleVal.X) * sin;
				transformation.M21 = (flippedVert ? -scaleVal.Y : scaleVal.Y) * (-sin);
				transformation.M22 = (flippedVert ? -scaleVal.Y : scaleVal.Y) * cos;
				transformation.M41 = (((flipAdjustment.X - originVal.X) * transformation.M11) + (flipAdjustment.Y - originVal.Y) * transformation.M21) + position.X;
				transformation.M42 = (((flipAdjustment.X - originVal.X) * transformation.M12) + (flipAdjustment.Y - originVal.Y) * transformation.M22) + position.Y; 
			}

			var offset           = Vector2.Zero;
			var firstGlyphOfLine = true;

			TextColor styleColor = color;

			bool styleRandom = false, 
			     styleBold = style.HasFlag(FontStyle.Bold), 
			     styleItalic = style.HasFlag(FontStyle.Italic), 
			     styleUnderline = style.HasFlag(FontStyle.Underline), 
			     styleStrikethrough = style.HasFlag(FontStyle.StrikeThrough), 
			     dropShadow = style.HasFlag(FontStyle.DropShadow);

			var blendFactor = sb.GraphicsDevice.BlendFactor;
			sb.GraphicsDevice.BlendFactor = Color.White * opacity;

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if(c == '\r') continue;
				
				if (c == '\n')
				{
					offset.X =  0.0f;
					offset.Y += LineSpacing;

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
					var glyph = GetGlyphOrDefault(c);


					if (firstGlyphOfLine)
					{
					//	offset.X += CharacterSpacing;
						firstGlyphOfLine = false;
					}

					//if (styleRandom)
					//{
					//	c = 
					//}

					var p = offset;

					if (dropShadow)
					{
						var shadowP = p + Vector2.One;
						
						if (styleBold)
						{
							var boldShadowP = Vector2.Transform(shadowP + Vector2.UnitX, transformation);

							sb.Draw(glyph.Texture, boldShadowP, styleColor.BackgroundColor * opacity, rotation, originVal, scaleVal, effects, layerDepth);
						}
						
						shadowP = Vector2.Transform(shadowP, transformation);

						sb.Draw(glyph.Texture, shadowP, styleColor.BackgroundColor * opacity, rotation, originVal, scaleVal, effects, layerDepth);
					}

					if (styleBold)
					{
						var boldP = Vector2.Transform(p + Vector2.UnitX, transformation);
						sb.Draw(glyph.Texture, boldP, styleColor.ForegroundColor * opacity, rotation, originVal, scaleVal, effects, layerDepth);
					}

					p = Vector2.Transform(p, transformation);

					sb.Draw(glyph.Texture, p, styleColor.ForegroundColor * opacity, rotation, originVal, scaleVal, effects, layerDepth);

					offset.X += glyph.Width + (styleBold ? 1 : 0) + CharacterSpacing;
				}
			}

			sb.GraphicsDevice.BlendFactor = blendFactor;
		}

        public IFontGlyph GetGlyphOrDefault(char character)
        {
            for (int i = 0; i < Glyphs.Length; i++)
            {
                if(Glyphs[i].Character == character) return Glyphs[i];
            }

            return DefaultGlyph;
        }
        
        private void LoadGlyphs(Bitmap bitmap, List<char> characters)
        {
            if (_isInitialised) return;

            var lockedBitmap = new LockBitmap(bitmap);
            lockedBitmap.LockBits();
            Color[] textureData = lockedBitmap.GetColorArray();
            lockedBitmap.UnlockBits();
            
            var textureWidth = bitmap.Width;
            var textureHeight = bitmap.Height;

            var cellWidth = (int)(textureWidth / (float)GridWidth);
            var cellHeight = (int)(textureHeight / (float)GridHeight);

            var charactersCount = characters.Count;

            var glyphs = new Glyph[charactersCount];

            for (int i = 0; i < charactersCount; i++)
            {
                int row = i / GridWidth;
                int col = i % GridWidth;
                
                // Scan the grid cell by pixel column, to determine the
                // width of the characters.
                int width = 0;
                bool columnIsEmpty = true;
                for (int x = cellWidth-1; x >= 0; x--)
                {
                    columnIsEmpty = true;

                    for (int y = cellHeight-1; y >= 0; y--)
                    {
                        // width * y + x
                        if (textureData[(textureWidth * (row * cellHeight + y)) + (col * cellWidth + x)].A != 0)
                        {
                            columnIsEmpty = false;
                        }
                    }

                    width = x;

                    if (!columnIsEmpty)
                    {
                        break;
                    }
                }
                
                
                var charWidth = (int) (0.5d + (width * (8.0f / cellWidth)) + 1);

                ++width;
                var bounds = new Rectangle(col * cellWidth, row * cellHeight, width, cellHeight);
                var textureSlice = Texture.Slice(bounds);

                var character = characters[i];

	            if (character == ' ')
	            {
		            charWidth = 4;
	            }

				var glyph = new Glyph(character, textureSlice, charWidth, cellHeight);

                Debug.WriteLine($"BitmapFont Glyph Loaded: {glyph}");
                glyphs[i] = glyph;
            }

            Glyphs = glyphs;
            DefaultGlyph = new Glyph('\x0000',Texture.Slice(0, 0, 0, 0), 8, 0);

            _isInitialised = true;
        }

        public struct Glyph : IFontGlyph
        {
            public char Character { get; }
            public ITexture2D Texture { get; }
            public float Width { get; }
            public float Height { get; }

            internal Glyph(char character, ITexture2D texture, float width, float height)
            {
                Character = character;
                Texture = texture;
                Width = width;
                Height = height;
            }

            public override string ToString()
            {
                return $"CharacterIndex={Character}, Glyph={Texture}, Width={Width}, Height={Height}";
            }
        }
    }
}
