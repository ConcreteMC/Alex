using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Enums;

namespace Alex.Gui.Controls.Menu
{
	public class UiMenuItem : UiControl
	{

		public UiLabel Label { get; }

		public Action Action { get; }

		public UiMenuItem(string text, Action action = null)
		{
			Label = new UiLabel(text);
			Action = action;
			
			HorizontalContentAlignment = HorizontalAlignment.Center;
			VerticalContentAlignment = VerticalAlignment.Center;

			Controls.Add(Label);
		}

	}
}
