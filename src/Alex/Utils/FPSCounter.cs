using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Utils
{
    public static class FpsCounter
    {
        private const string BiosFontData = "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";
        private static BitmapFont _biosFont;
        private static bool _initialized;
        private static GraphicsDevice _graphics;
        private static SpriteBatch _batch;
        private static void Initialize(GraphicsDevice graphics)
        {
            if (_initialized == false)
            {
                _graphics = graphics;

                _initialized = true;
                byte[] pngBytes = Convert.FromBase64String(BiosFontData);
                _biosFont = new BitmapFont(Texture2D.FromStream(_graphics, new MemoryStream(pngBytes)), 8);
                _batch = new SpriteBatch(_graphics);
            }
        }
        private static readonly Dictionary<int, int> FpsIncrement = new Dictionary<int, int>();
        private static readonly Dictionary<int, int> FpsValue = new Dictionary<int, int>();
        private static readonly Dictionary<int, Stopwatch> FpsTimer = new Dictionary<int, Stopwatch>();
        public static void Tick(int index)
        {
            if (FpsIncrement.ContainsKey(index) == false)
            {
                FpsValue[index] = 0;
                FpsIncrement[index] = 0;
                FpsTimer[index] = new Stopwatch();
                FpsTimer[index].Start();
            }

            FpsIncrement[index] += 1;
            if (FpsTimer[index].Elapsed.TotalMilliseconds > 1000.0f)
            {
                FpsTimer[index].Restart();
                FpsValue[index] = FpsIncrement[index];
                FpsIncrement[index] = 0;
            }
        }

        public static void Render(GraphicsDevice graphics, float xpos = 16, float ypos = 16, int index = -1, string label = "FPS")
        {
            Render(graphics, Color.White, xpos, ypos, index, label);
        }

        public static void Render(GraphicsDevice graphics, Color color, float xpos = 16, float ypos = 16, int index = -1, string label = "FPS")
        {
            if (index == -1)
                Tick(index);

            if (_initialized == false)
                Initialize(graphics);

            int val = 0;
            if (FpsValue.ContainsKey(index))
                val = FpsValue[index];

            _batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
            _biosFont.Draw(_batch, Convert.ToString(val, CultureInfo.InvariantCulture) + " " + label, xpos, ypos, color);
            _batch.End();
        }

        internal class BitmapFont
        {
            private readonly Texture2D _texture;
            private static bool _mapInitialized;
            private static readonly char[] Characters = {
            ' ', '!', '\"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '>', '=', '<', '?',
            '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O',
            'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_',
            '`', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', '|', '}', '~'
        };
            private static readonly int[] CharacterMap = new int[255];

            private const int KTilesPerRow = 16;
            private readonly int _tileSize;
            public BitmapFont(Texture2D image, int size)
            {
                _texture = image;
                _tileSize = size;
                if (_mapInitialized == false)
                {
                    //Map characters for easy access
                    for (int i = 0; i < 255; i++)
                    {
                        char c = (char)i;
                        CharacterMap[i] = '[';
                        for (int iChar = 0; iChar < Characters.Length; iChar++)
                        {
                            if (Characters[iChar] == c)
                            {
                                CharacterMap[i] = iChar;
                                break;
                            }
                        }
                    }
                    _mapInitialized = true;
                }
            }

            public void Draw(SpriteBatch batch, string text, float xpos, float ypos, Color c)
            {
                float xOff = 0.0f;
                foreach (char t in text)
                {
                    if (t == '\n')
                    {
                        xOff = 0.0f;
                        continue;
                    }
                    int charIndex = CharacterMap[t];

                    int uvX = ((charIndex % KTilesPerRow) * _tileSize);
                    int uvY = ((charIndex / KTilesPerRow) * _tileSize);
                    batch.Draw(_texture, new Vector2(xpos + xOff, ypos), new Rectangle(uvX, uvY, _tileSize, _tileSize), c, 0.0f, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 1.0f);
                    xOff += _tileSize;
                }
            }
        }


    }
}
