using System;
using Alex.Graphics.UI.Layout;

namespace Alex.Graphics.UI.Controls.Menu
{
	public class UiMenu : UiStackPanel
	{

		public void AddMenuItem(string text, Action action = null)
		{
			AddChild(new UiMenuItem(text, action));
		}

	}
}
