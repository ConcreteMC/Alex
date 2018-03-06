using System;
using Alex.Graphics.UI.Layout;

namespace Alex.Graphics.UI.Controls.Menu
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
