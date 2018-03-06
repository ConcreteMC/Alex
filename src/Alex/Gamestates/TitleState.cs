using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics;
using Alex.Graphics.UI;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Controls.Menu;
using Alex.Graphics.UI.Layout;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates
{
	public class TitleState : GameState
	{

		public TitleState(Alex alex) : base(alex)
		{
		}

		protected override void OnLoad(RenderArgs args)
		{
			Gui.ClassName = "TitleScreenRoot";

			var menuWrapper = new UiPanel()
			{
				CustomStyle =
				{
					Width   = 250,
					Margin  = new Thickness(100, 0, 100, 0),
					Padding = new Thickness(10),
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center
				}
			};
			var stackMenu = new UiMenu()
			{
				ClassName = "TitleScreenMenu",
				CustomStyle =
				{
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center
				}
			};

			stackMenu.AddMenuItem("Play", () => { });
			stackMenu.AddMenuItem("Debug World", () => { });
			stackMenu.AddMenuItem("Options", () => { });
			stackMenu.AddMenuItem("Exit Game", () => { Alex.Exit(); });

			menuWrapper.Controls.Add(stackMenu);

			Gui.Controls.Add(menuWrapper);

			var logo = new UiElement()
			{
				ClassName = "TitleScreenLogo",
			};
			Gui.Controls.Add(logo);

			Alex.IsMouseVisible = true;
		}


	}

	class TitleSkyBoxBackground
	{

	}
}
