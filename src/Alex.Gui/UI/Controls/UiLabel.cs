using Alex.Graphics.UI.Abstractions;
using Alex.Graphics.UI.Rendering;
using Alex.Graphics.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.UI.Controls
{
	public class UiLabel : UiElement, ITextElement
	{
		private string _text;

		public string Text
		{
			get { return _text; }
			set
			{
				_text = value;
				MarkLayoutDirty();
			}
		}

		public UiLabel(string text)
		{
			Text = text;
		}

		protected internal override Point GetAutoSize()
		{
			if (Style.TextFont != null && !string.IsNullOrWhiteSpace(Text))
			{
				return base.GetAutoSize().Max(Style.TextFont?.MeasureString(Text).ToPoint() ?? Point.Zero);
			}
			return base.GetAutoSize();
		}

	}
}
