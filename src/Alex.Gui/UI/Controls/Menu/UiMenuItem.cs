using System;
using Alex.Graphics.UI.Abstractions;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Input.Listeners;
using Alex.Graphics.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Controls.Menu
{
	public class UiMenuItem : UiControl, ITextElement
	{
		private string _text;

		public string Text
		{
			get => _text;
			set
			{
				_text = value;
				MarkLayoutDirty();
			}
		}

		public Action Action { get; }

		public UiMenuItem(string text, Action action = null)
		{
			Text = text;
			Action = action;
		}

		protected internal override Point GetAutoSize()
		{
			var textSize = Style.TextFont?.MeasureString(Text).ToPoint() ?? Point.Zero;
			var baseSize = base.GetAutoSize();

			return baseSize.Max(textSize) + new Point(Padding.Horizontal, Padding.Vertical);
		}

		protected override void OnMouseUp(MouseEventArgs args)
		{
			if (IsMouseOver)
			{
				Action?.Invoke();
			}
		}

	}
}
