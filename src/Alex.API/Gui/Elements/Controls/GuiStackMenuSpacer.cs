using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.API.Gui.Elements.Controls
{
	public class GuiStackMenuSpacer : GuiElement
	{
		public GuiStackMenuSpacer()
		{
			Margin = new Thickness(5, 5, 5, 5);
		}

		protected override void OnDraw(GuiSpriteBatch graphics, GameTime gameTime)
		{
			//	base.OnDraw(graphics, gameTime);
			graphics.DrawLine(new Vector2(RenderBounds.Left, RenderBounds.Height / 2f), RenderBounds.Width, 0f, TextColor.DarkGray.ForegroundColor, 1);
		}
	}
}
