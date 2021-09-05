using System.Collections.Generic;
using Alex.Common.Graphics.Typography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RocketUI
{
    public class SDFFont : IFont
    {
        public class CharRecord
        {
            public int ID;

            public int X;
            public int Y;
            public int Width;
            public int Height;

            public float xoffset;
            public float yoffset;
            public float xadvance;

            public CharRecord(int id, int x, int y, int w, int h, float xo, float yo, float xa)
            {
                ID = id;
                X = x;
                Y = y;
                Width = w;
                Height = h;
                xoffset = xo;
                yoffset = yo;
                xadvance = xa;
            }
        };
        
        public  float                       Scale { get; set; }
        private Texture2D                   _texture;
        private Effect                      _effect;
        private Dictionary<int, CharRecord> chars = new Dictionary<int, CharRecord>();
        private GlyphBatch                  gbatch;

        public SDFFont(GraphicsDevice device, Texture2D tex, Effect eff, float scale = 0.4f)
        {
            Scale = scale;
            _texture = tex;
            _effect = eff;
        }

        public IReadOnlyCollection<char> Characters { get; }

        public Vector2 MeasureString(string text)
        {
            Vector2 result = new Vector2(0, 0);
            foreach (char c in text)
            {
                CharRecord cr = chars[(int)c];
                float      ch = cr.Height;
                if (ch > result.Y)
                    result.Y = ch;
                result.X += cr.xadvance;
            }

            return result * Scale;
        }

        public void DrawString(SpriteBatch sb, string text, Vector2 position, Color color, FontStyle style = FontStyle.None,
            Vector2?                scale   = null, float opacity = 1, float rotation = 0, Vector2? origin = null,
            SpriteEffects           effects = SpriteEffects.None, float layerDepth = 0)
        {
            
        }
    }
}