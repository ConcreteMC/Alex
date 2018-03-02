using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gui.Controls
{
	public class UiLabel : UiControl
	{

		public string Text { get; set; }

		public SpriteFont Font { get; set; }


		public UiLabel(string text)
		{
			Text = text;
		}

		protected override void OnLayout()
		{
			Size = Font.MeasureString(Text).ToPoint();
			base.OnLayout();
		}

		protected override void OnDraw(GameTime gameTime, GuiRenderer renderer)
		{
			base.OnDraw(gameTime, renderer);

			renderer.DrawText(ClientBounds, Text, Font, ForegroundColor);
		}
	}
}
