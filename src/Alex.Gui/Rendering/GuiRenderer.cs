using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Rendering
{
	public class GuiRenderer
	{

		public GraphicsDevice Graphics { get; private set; }

		public int ScreenWidth { get; private set; }
		public int ScreenHeight { get; private set; }

		public Matrix Projection { get; set; }

		private SpriteBatch _spriteBatch;


		public GuiRenderer(GraphicsDevice graphicsDevice, int screenWidth, int screenHeight)
		{
			_spriteBatch = new SpriteBatch(graphicsDevice);
			Graphics = graphicsDevice;

			ScreenWidth = screenWidth;
			ScreenHeight = screenHeight;
		}

		public void Begin()
		{
			_spriteBatch.Begin(samplerState: SamplerState.PointClamp);
		}

		public void End()
		{
			_spriteBatch.End();
		}

		public void DrawRectangle(Rectangle bounds, Color color)
		{
			var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
			texture.SetData(new Color[] { color });

			DrawRectangle(bounds, texture);
		}

		public void DrawRectangle(Rectangle bounds, Texture2D texture)
		{
			_spriteBatch.Draw(texture, bounds, Color.White);
		}

		public void DrawText(Rectangle bounds, string text, SpriteFont font, Color color)
		{
			_spriteBatch.DrawString(font, text, bounds.Location.ToVector2(), color);
		}
	}
}
