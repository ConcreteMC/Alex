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
		private string _text;

		public string Text
		{
			get { return _text; }
			set { _text = value; UpdateSize(); }
		}

		private SpriteFont _font;

		public SpriteFont Font
		{
			get { return _font; }
			set { _font = value; UpdateSize(); }
		}

		public int ShadowSize { get; set; } = 1;
		public Color ShadowColor { get; set; } = Color.Black;


		public UiLabel(string text)
		{
			Text = text;
		}

		public override void UpdateSize()
		{
			if (Font != null && !string.IsNullOrWhiteSpace(Text))
			{
				var s = Font.MeasureString(Text).ToPoint();
				ActualHeight = s.Y;
				ActualWidth = s.X;
			}
			else
			{
				ActualHeight = 0;
				ActualWidth = 0;
			}
		}

		protected override void OnDraw(GameTime gameTime, GuiRenderer renderer)
		{
			base.OnDraw(gameTime, renderer);

			if (ShadowSize > 0)
			{
				var bounds = ClientBounds;
				for (int i = 0; i < ShadowSize; i++)
				{
					bounds = new Rectangle(bounds.Location + new Point(2,2), bounds.Size);
					renderer.DrawText(bounds, Text, Font, ShadowColor);
				}
			}

			renderer.DrawText(ClientBounds, Text, Font, ForegroundColor);
		}
	}
}
