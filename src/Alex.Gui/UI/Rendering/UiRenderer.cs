using System;
using Alex.Graphics.Textures;
using Alex.Graphics.UI.Abstractions;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Themes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.UI.Rendering
{
	public class UiRenderer
	{

		public UiManager UiManager { get; }
		public UiTheme Theme => UiManager.Theme;

		private GraphicsDevice Graphics { get; }
		private SpriteBatch SpriteBatch { get; }


		public int VirtualWidth { get; private set; }
		public int VirtualHeight { get; private set; }
		public int ScaleFactor { get; private set; }

		public Matrix ScaleMatrix { get; private set; }
		public Matrix PointToScreenMatrix { get; private set; }

		private RasterizerState _rasteriserState = null;



		public UiRenderer(UiManager uiManager, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			UiManager = uiManager;
			SpriteBatch = spriteBatch;
			Graphics = graphicsDevice;

			UpdateViewport();

			//_rasteriserState = new RasterizerState()
			//{
			//ScissorTestEnable = true,
			//CullMode = CullMode.None,
			//};
		}

		public void SetVirtualSize(int width, int height, int scaleFactor)
		{
			VirtualWidth = width;
			VirtualHeight = height;
			ScaleFactor = scaleFactor;
			UpdateViewport();
		}

		private void UpdateViewport()
		{
			var viewport = Graphics.Viewport;
			var scaleX = (float)viewport.Width / VirtualWidth;
			var scaleY = (float)viewport.Height / VirtualHeight;

			ScaleMatrix = Matrix.CreateScale(scaleX, scaleY, 1.0f);
			PointToScreenMatrix = Matrix.Invert(ScaleMatrix);
		}

		public Point PointToScreen(Point point)
		{
			return Vector2.Transform(point.ToVector2(), PointToScreenMatrix).ToPoint();
		}

		public void BeginDraw()
		{
			SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, _rasteriserState, null, ScaleMatrix);
		}

		public void EndDraw()
		{
			SpriteBatch.End();
		}



		public void DrawElement(UiElement element)
		{
			var args = new UiElementRenderArgs(element);

			DrawElementStyle(element, args);

			// Debug Bounding Boxes
			DrawDebugBoundingBoxes(element);
		}

		private void DrawDebugBoundingBoxes(UiElement element)
		{
			DrawRectangle(element.ClientBounds, Color.Blue);
			DrawRectangle(element.OuterBounds, Color.Red);
			DrawRectangle(element.Bounds, Color.Green);
		}

		private void DrawElementStyle(UiElement element, UiElementRenderArgs args)
		{
			var style = args.Style;

			if (style.BackgroundColor.A > 0)
			{
				FillRectangle(element.Bounds, style.BackgroundColor);
			}

			if (style.Background != null)
			{
				DrawNinePatch(element.Bounds, style.Background, style.BackgroundRepeat);
			}

			if (element is ITextElement textElement)
			{
				DrawTextElement(textElement, args);
			}
		}

		private void DrawTextElement(ITextElement element, UiElementRenderArgs args)
		{
			var style = args.Style;
			if (!(style.TextFont != null && style.TextColor.A > 0 && !string.IsNullOrWhiteSpace(element.Text))) return;

			var font = style.TextFont;

			var textSize = font.MeasureString(element.Text);

			var pos = Vector2.Zero;
			if (style.HorizontalContentAlignment == HorizontalAlignment.Center)
			{
				pos.X = (args.ContentBounds.Width - textSize.X) / 2f;
			}
			else if (style.HorizontalContentAlignment == HorizontalAlignment.Right)
			{
				pos.X = (args.ContentBounds.Width - textSize.X);
			}

			if (style.VerticalContentAlignment == VerticalAlignment.Center)
			{
				pos.Y = (args.ContentBounds.Height - textSize.Y) / 2f;
			}
			else if (style.VerticalContentAlignment == VerticalAlignment.Bottom)
			{
				pos.Y = (args.ContentBounds.Height - textSize.Y);
			}

			pos += args.Position;

			if (style.TextShadowSize > 0)
			{
				for (int i = 0; i < style.TextShadowSize; i++)
				{
					SpriteBatch.DrawString(style.TextFont, element.Text, pos + new Vector2(i, i), style.TextShadowColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
				}
			}

			SpriteBatch.DrawString(style.TextFont, element.Text, pos, style.TextColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
		}

		public void FillRectangle(Rectangle bounds, Color color)
		{
			var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
			texture.SetData(new Color[] { color });

			FillRectangle(bounds, texture, TextureRepeatMode.Stretch);
		}

		public void DrawNinePatch(Rectangle bounds, NinePatchTexture ninePatchTexture, TextureRepeatMode repeatMode)
		{
			if (ninePatchTexture.NineSliceSize == 0)
			{
				FillRectangle(bounds, ninePatchTexture.Texture, repeatMode);
				return;
			}

			var texture = ninePatchTexture.Texture;
			var patchSize = ninePatchTexture.NineSliceSize;
			var projectedPatchSize = patchSize;

			var innerBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			innerBounds.Inflate(-projectedPatchSize, -projectedPatchSize);

			var tBounds = texture.Bounds;
			tBounds.Inflate(-patchSize, -patchSize);

			// Top Left
			SpriteBatch.Draw(texture, new Rectangle(bounds.Left, bounds.Top, projectedPatchSize, projectedPatchSize), new Rectangle(0, 0, tBounds.Left, tBounds.Top), Color.White);

			// Top Right
			SpriteBatch.Draw(texture, new Rectangle(innerBounds.Right, bounds.Top, projectedPatchSize, projectedPatchSize), new Rectangle(tBounds.Right, 0, tBounds.Left, tBounds.Top), Color.White);


			// Bottom Left
			SpriteBatch.Draw(texture, new Rectangle(bounds.Left, innerBounds.Bottom, patchSize, patchSize), new Rectangle(0, tBounds.Bottom, tBounds.Left, tBounds.Top), Color.White);

			// Bottom Right
			SpriteBatch.Draw(texture, new Rectangle(innerBounds.Right, innerBounds.Bottom, patchSize, patchSize), new Rectangle(tBounds.Right, tBounds.Bottom, tBounds.Left, tBounds.Top), Color.White);


			// Left Middle
			SpriteBatch.Draw(texture, new Rectangle(bounds.Left, innerBounds.Top, patchSize, innerBounds.Height), new Rectangle(0, tBounds.Top, tBounds.Left, tBounds.Height), Color.White);

			// Right Middle
			SpriteBatch.Draw(texture, new Rectangle(innerBounds.Right, innerBounds.Top, patchSize, innerBounds.Height), new Rectangle(tBounds.Right, tBounds.Top, tBounds.Left, tBounds.Height), Color.White);


			// Top Middle
			SpriteBatch.Draw(texture, new Rectangle(innerBounds.Left, bounds.Top, innerBounds.Width, patchSize), new Rectangle(tBounds.Left, 0, tBounds.Width, tBounds.Top), Color.White);

			// Bottom Middle
			SpriteBatch.Draw(texture, new Rectangle(innerBounds.Left, innerBounds.Bottom, innerBounds.Width, patchSize), new Rectangle(tBounds.Left, tBounds.Bottom, tBounds.Width, tBounds.Top), Color.White);


			// Middle Middle
			SpriteBatch.Draw(texture, innerBounds, tBounds, Color.White);
		}

		public void FillRectangle(Rectangle bounds, Texture2D texture, TextureRepeatMode repeatMode)
		{
			if (repeatMode == TextureRepeatMode.NoRepeat)
			{
				SpriteBatch.Draw(texture, new Rectangle(bounds.Location, new Point(texture.Width, texture.Height)), texture.Bounds, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
			}
			else if (repeatMode == TextureRepeatMode.Stretch)
			{
				SpriteBatch.Draw(texture, bounds, Color.White);
			}
			else if (repeatMode == TextureRepeatMode.ScaleToFit)
			{

			}
			else if (repeatMode == TextureRepeatMode.Tile)
			{
				var repeatX = Math.Ceiling((float)bounds.Width / texture.Width);
				var repeatY = Math.Ceiling((float)bounds.Height / texture.Height);

				for (int i = 0; i < repeatX; i++)
				{
					for (int j = 0; j < repeatY; j++)
					{
						var p = bounds.Location.ToVector2() + new Vector2(i * texture.Width, j * texture.Height);
						SpriteBatch.Draw(texture, p, texture.Bounds, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
					}
				}
			}
		}

		public void DrawRectangle(Rectangle bounds, Color color, int thickness = 1)
		{
			var texture = new Texture2D(Graphics, 1, 1, false, SurfaceFormat.Color);
			texture.SetData(new Color[] { color });

			// Top
			SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);

			// Right
			SpriteBatch.Draw(texture, new Rectangle(bounds.X + bounds.Width, bounds.Y, thickness, bounds.Height), color);

			// Bottom
			SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width, thickness), color);

			// Left
			SpriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);

		}

		public void DrawText(Rectangle bounds, string text, SpriteFont font, Color color)
		{
			if (font != null && !string.IsNullOrWhiteSpace(text))
			{
				SpriteBatch.DrawString(font, text, bounds.Location.ToVector2(), color);
			}
		}
	}
}
