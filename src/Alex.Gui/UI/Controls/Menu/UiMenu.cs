using System;
using Alex.Graphics.UI.Layout;

namespace Alex.Graphics.UI.Controls.Menu
{
	public class UiMenu : UiStackPanel
	{

		public void AddMenuItem(string text, Action action = null)
		{
			var menuItem = new UiMenuItem(text, action);
			AddChild(menuItem);
		}

	}
}
