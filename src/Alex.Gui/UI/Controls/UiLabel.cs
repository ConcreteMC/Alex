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
				if (_text == value) return;
				_text = value;
				OnPropertyChanged();
				MarkLayoutDirty();
			}
		}

		public UiLabel(string text)
		{
			Text = text;
		}

	}
}
