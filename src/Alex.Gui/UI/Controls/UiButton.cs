using System;
using System.Reflection.Emit;
using Alex.Graphics.UI.Abstractions;
using Alex.Graphics.UI.Input.Listeners;

namespace Alex.Graphics.UI.Controls
{
	public class UiButton : UiControl, ITextElement
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

		public Action Action { get; }

		public UiButton(string text, Action action)
		{
			Text = text;
			Action = action;
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
