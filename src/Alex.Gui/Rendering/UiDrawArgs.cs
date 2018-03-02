using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Rendering
{
	public class UiDrawArgs
	{

		public SpriteBatch SpriteBatch { get; set; }

		public GameTime GameTime { get; set; }

		public UiDrawArgs(SpriteBatch spriteBatch, GameTime gameTime)
		{
			SpriteBatch = spriteBatch;
			GameTime = gameTime;
		}

		public void DrawRectangle(Rectangle rectangle, Color color)
		{

		}
	}
}
