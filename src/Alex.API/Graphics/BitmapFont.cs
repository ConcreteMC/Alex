using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.API.Graphics
{
    public class BitmapFont
    {

        public char[] Characters { get; }

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
            LoadGlyphSizes(bitmap, characters);
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

            DefaultGlyph = new Glyph() { Width = 0, Height = 8 };
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
                    continue;
                }

                if (c == '\x00A7')
                {
                    // Formatting
                    i++; // skip next char
                    continue;
                }

                var glyph = GetGlyphOrDefault(c);

                if (firstGlyphOfLine)
                {
                    offset.X += CharacterSpacing;
                }
                    
                firstGlyphOfLine = false;

                offset.X += glyph.Width + CharacterSpacing;

                finalLineHeight = Math.Max(finalLineHeight, glyph.Height);
                width = Math.Max(width, offset.X);
                
            }

            size.X = width;
            size.Y = offset.Y + finalLineHeight;
        }

        public Glyph GetGlyphOrDefault(char character)
        {

            for (int i = 0; i < Glyphs.Length; i++)
            {
                if(Glyphs[i].Character == character) return Glyphs[i];
            }

            return DefaultGlyph;
        }
        
        private void LoadGlyphSizes(Bitmap bitmap, List<char> characters)
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

                ++width;

                var charWidth = (int) (0.5d + (width * (8.0f / cellWidth)) + 1);

                var bounds = new Rectangle(col * cellWidth, row * cellHeight, width, cellHeight);
                var textureSlice = Texture.Slice(bounds);

                
                var character = characters[i];

                var glyph = new Glyph()
                {
                    Character = character,
                    TextureSlice = textureSlice,
                    Width = charWidth,
                    Height = cellHeight
                };

                Debug.WriteLine($"BitmapFont Glyph Loaded: {glyph}");
                glyphs[i] = glyph;
            }

            Glyphs = glyphs;
            DefaultGlyph = new Glyph()
            {
                Character = '\x0000',
                Height = 8,
                TextureSlice = Texture.Slice(0, 0, 0, 0),
                Width = 0,
            };

            _isInitialised = true;
        }

        public struct Glyph
        {
            public char Character;
            public TextureSlice2D TextureSlice;
            public float Width;
            public float Height;

            public override string ToString()
            {
                return $"CharacterIndex={Character}, Glyph={TextureSlice}, Width={Width}, Height={Height}";
            }
        }
    }
}
