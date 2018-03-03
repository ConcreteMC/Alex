using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Layout;

namespace Alex.Gui.Controls.Menu
{
	public class UiMenu : UiStackPanel
	{

		public void AddMenuItem(string text, Action action = null)
		{
			var item = new UiMenuItem(text, action);
			Controls.Add(item);
		}

	}
}
