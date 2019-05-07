using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Gui.Elements;
using WinApi.Windows;

namespace Alex.Utils
{
	public static class GuiExtensions
	{
		public static void SetDefaultLinkHandler(this GuiTextElement element)
		{
			element.OnLinkClicked += (sender, e) =>
			{
				//TODO
			};
		}
	}
}
