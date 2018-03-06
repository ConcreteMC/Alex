using System;

namespace Alex.Graphics.UI.Controls
{
	public class UiButton : UiControl
	{

		public UiLabel Label { get; }

		public Action Action { get; }

		public UiButton(string text, Action action)
		{
			Label = new UiLabel(text);
			Action = action;

			Controls.Add(Label);
		}

	}
}
