using System;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gui.Elements
{
	public class LoadingIndicator : RocketElement
	{
		public Color ForegroundColor { get; set; } = Color.Black;
		public Color BackgroundColor { get; set; } = Color.White;

		private double _progress = 0f;
		private bool _add = true;
		public LoadingIndicator()
		{
			
		}
		
	//	private void Draw(GuiSpriteBatch graphics, )

		/// <inheritdoc />
		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			base.OnDraw(graphics, gameTime);

			if (_add)
			{
				_progress += gameTime.ElapsedGameTime.TotalSeconds;
				_progress = Math.Clamp(_progress, 0d, 1d);
				
				if (_progress >= 1d)
				{
					_add = false;
				}
			}
			else
			{
				_progress -= gameTime.ElapsedGameTime.TotalSeconds;
				_progress = Math.Clamp(_progress, 0d, 1d);
				
				if (_progress <= 0d)
				{
					_add = true;
				}
			}

			graphics.FillRectangle(RenderBounds, BackgroundColor);
			var xOffset = (_progress * RenderBounds.Width);

			graphics.FillRectangle(new Rectangle(RenderBounds.X + (int) xOffset, RenderBounds.Y, RenderBounds.Height, RenderBounds.Height), ForegroundColor);
		}
	}
}