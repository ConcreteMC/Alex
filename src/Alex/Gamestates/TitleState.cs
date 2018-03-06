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

			var menuWrapper = new UiPanel();
			var stackMenu = new UiMenu()
			{
				ClassName = "TitleScreenMenu"
			};

			stackMenu.AddMenuItem("Play", () => { });
			stackMenu.AddMenuItem("Debug World", () => { });
			stackMenu.AddMenuItem("Options", () => { });
			stackMenu.AddMenuItem("Exit Game", () => { Alex.Exit(); });

			menuWrapper.AddChild(stackMenu);

			Gui.AddChild(menuWrapper);

			var logo = new UiElement()
			{
				ClassName = "TitleScreenLogo",
			};
			Gui.AddChild(logo);

			Alex.IsMouseVisible = true;
		}


	}

	class TitleSkyBoxBackground
	{

	}
}
