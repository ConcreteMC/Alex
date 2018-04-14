using System;
using System.Drawing;
using System.Drawing.Imaging;
using Alex.API;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.ResourcePackLib
{
    public class BitmapFontRenderer : IFontRenderer
	{
		public int DrawString(SpriteBatch sb,         string  text, Vector2 position, Color color,
		                      bool        dropShadow, Vector2 scale,
		                      float       rotation,   Vector2 origin, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0)
		{
			throw new NotImplementedException();
		}

		public int DrawString(SpriteBatch sb, string text, Vector2 position, Color color, bool dropShadow, Vector2 scale)
		{
			throw new NotImplementedException();
		}

		public int DrawString(SpriteBatch sb, string text, Vector2 position, Color color, bool dropShadow)
		{
			throw new NotImplementedException();
		}

		public int DrawString(SpriteBatch sb, string text, int x, int y, Color color)
		{
			throw new NotImplementedException();
		}

		public int GetCharWidth(char character)
		{
			throw new NotImplementedException();
		}

		public int GetStringWidth(string text)
		{
			throw new NotImplementedException();
		}

		public Vector2 GetStringSize(string text, Vector2 scale)
		{
			throw new NotImplementedException();
		}
	}
}
