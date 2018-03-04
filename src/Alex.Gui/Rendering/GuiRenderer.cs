using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Themes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Rendering
{
	public class GuiRenderer
	{

		public UiTheme Theme { get; set; } = new UiTheme();

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

		public void DrawElement(UiElement element)
		{
			DrawElementStyle(element, element.Style);
			
			// Debug Bounding Boxes
			//DrawDebugBoundingBoxes(renderer);
		}

		private void DrawDebugBoundingBoxes(UiElement element, GuiRenderer renderer)
		{
			renderer.DrawRectangle(element.ClientBounds,Color.Blue);
			renderer.DrawRectangle(element.OuterBounds,Color.Red);
			renderer.DrawRectangle(element.Bounds, Color.Green);
		}

		private void DrawElementStyle(UiElement element, UiElementStyle style)
		{
			if (style.BackgroundColor.HasValue)
			{
				FillRectangle(element.Bounds, style.BackgroundColor.Value);
			}

			if (style.Background.HasValue)
			{
				DrawNinePatch(element.Bounds, style.Background);
			} 
		}

		public void DrawNinePatch(Rectangle bounds, NinePatchTexture ninePatchTexture)
		{
			if (ninePatchTexture.NineSliceSize == 0)
			{
				FillRectangle(bounds, ninePatchTexture.Texture);
				return;
			}


			var texture = ninePatchTexture.Texture;
			var patchSize = ninePatchTexture.NineSliceSize;
			var x2PatchSize = patchSize * 2;

			var innerBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			innerBounds.Inflate(-patchSize, -patchSize);

			// Top Left
			_spriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, patchSize, patchSize), new Rectangle(0, 0, patchSize, patchSize), Color.White);

			// Top Right
			_spriteBatch.Draw(texture, new Rectangle(innerBounds.Right, bounds.Top, patchSize, patchSize), new Rectangle(texture.Width - patchSize, 0, patchSize, patchSize), Color.White);


			// Bottom Left
			_spriteBatch.Draw(texture, new Rectangle(bounds.Left, innerBounds.Bottom, patchSize, patchSize), new Rectangle(0, texture.Height - patchSize, patchSize, patchSize), Color.White);

			// Bottom Right
			_spriteBatch.Draw(texture, new Rectangle(innerBounds.Right, innerBounds.Bottom, patchSize, patchSize), new Rectangle(texture.Width - patchSize, texture.Height - patchSize, patchSize, patchSize), Color.White);

			
			// Top Middle
			_spriteBatch.Draw(texture, new Rectangle(innerBounds.Left, bounds.Top, innerBounds.Width, patchSize), new Rectangle(patchSize, 0, texture.Width - x2PatchSize, patchSize), Color.White);

			// Bottom Middle
			_spriteBatch.Draw(texture, new Rectangle(innerBounds.Left, innerBounds.Bottom, innerBounds.Width, patchSize), new Rectangle(patchSize, texture.Height - patchSize, texture.Width - x2PatchSize, patchSize), Color.White);
			

			// Left Middle
			_spriteBatch.Draw(texture, new Rectangle(bounds.Left, innerBounds.Top, patchSize, innerBounds.Height), new Rectangle(patchSize, texture.Height - patchSize, texture.Width - x2PatchSize, patchSize), Color.White);
			
			// Right Middle
			_spriteBatch.Draw(texture, new Rectangle(innerBounds.Right, innerBounds.Top, patchSize, innerBounds.Height), new Rectangle(patchSize, texture.Height - patchSize, texture.Width - x2PatchSize, patchSize), Color.White);

			// Middle Middle
			_spriteBatch.Draw(texture, innerBounds, new Rectangle(patchSize, patchSize, texture.Width - x2PatchSize, texture.Height - x2PatchSize), Color.White);
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
