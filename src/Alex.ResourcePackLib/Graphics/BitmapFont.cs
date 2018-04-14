using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.ResourcePackLib.Graphics
{
    public class BitmapFont
    {
        public int GridWidth  { get; }
        public int GridHeight { get; }
        
        public Texture2D Texture { get; }
        public Glyph[] Glyphs { get; }

        private bool _isInitialised = false;

        public BitmapFont(Texture2D texture, int gridSize, List<char> characters) : this(texture, gridSize, gridSize, characters)
        {

        }

        public BitmapFont(Texture2D texture, int gridWidth, int gridHeight, List<char> characters)
        {
            Texture = texture;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
        }

        private void LoadGlyphSizes(List<char> characters)
        {
            var w = Texture.Width / GridWidth;
            var h = Texture.Height / GridHeight;
            Color[] textureData = new Color[Texture.Width * Texture.Height];
            
            var maxIndex = characters.Count;

            for (int i = 0; i < maxIndex; i++)
            {
                int j = i % GridWidth;
                int k = i - j;

                
                // Scan the grid cell by pixel column, to determine the
                // width of the characters.
                int width = 0;
                bool columnIsEmpty = true;
                for (int x = w; x >= 0; x--)
                {
                    columnIsEmpty = true;

                    for (int y = h; y >= 0; y--)
                    {
                        if (textureData[(j * w + x) + (k * h + y) * Texture.Width].A != 0)
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

                var charWidth = (int) (0.5d + (width * (8.0f / w)) + 1);

                var bounds = new Rectangle(GridWidth, GridHeight, w, h);
                var textureSlice = Texture.Slice(bounds);

                
                var character = characters[i];

                var glyph = new Glyph()
                {
                    Character = character,
                    TextureSlice = textureSlice,
                    Width = charWidth
                };

                Glyphs[i] = glyph;
            }

            _isInitialised = true;
        }

        public struct Glyph
        {
            public char Character;
            public TextureSlice2D TextureSlice;
            public float Width;

            public override string ToString()
            {
                return $"CharacterIndex={Character}, Glyph={TextureSlice}, Width={Width}";
            }
        }
    }
}
