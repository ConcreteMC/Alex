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

		private RasterizerState _rasteriserState;

		private SpriteBatch _spriteBatch;


		public GuiRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int screenWidth, int screenHeight)
		{
			_spriteBatch = spriteBatch;
			Graphics = graphicsDevice;

			ScreenWidth = screenWidth;
			ScreenHeight = screenHeight;

			_rasteriserState = new RasterizerState()
			{
				ScissorTestEnable = true
			};
		}

		public void ResetScreenSize(int screenWidth, int screenHeight)
		{
			ScreenWidth = screenWidth;
			ScreenHeight = screenHeight;
		}

		public void Begin()
		{
			_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, _rasteriserState, null, Matrix.Identity);
		}

		public void End()
		{
			_spriteBatch.End();
		}

		public void FillRectangle(Rectangle bounds, Color color)
		{
			var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
			texture.SetData(new Color[] { color });

			FillRectangle(bounds, texture);
		}

		public void FillRectangle(Rectangle bounds, Texture2D texture)
		{
			_spriteBatch.Draw(texture, bounds, Color.White);
		}

		public void DrawRectangle(Rectangle bounds, Color color, int thickness = 1)
		{
			var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
			texture.SetData(new Color[] { color });

			// Top
			_spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
			
			// Right
			_spriteBatch.Draw(texture, new Rectangle(bounds.X + bounds.Width, bounds.Y, thickness, bounds.Height), color);

			// Bottom
			_spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width, thickness), color);

			// Left
			_spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
			
		}

		public void DrawText(Rectangle bounds, string text, SpriteFont font, Color color)
		{
			_spriteBatch.DrawString(font, text, bounds.Location.ToVector2(), color);
		}
	}
}
