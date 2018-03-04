using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui;
using Alex.Gui.Common;
using Alex.Gui.Controls;
using Alex.Gui.Controls.Menu;
using Alex.Gui.Enums;
using Alex.Gui.Layout;
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
				Width = 250,
				Margin = new Thickness(100, 0, 100, 0),
				Padding = new Thickness(10),
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center
			};
			var stackMenu = new UiMenu()
			{
				HorizontalContentAlignment = HorizontalAlignment.Center,
				VerticalContentAlignment = VerticalAlignment.Center
			};

			var playButton = new UiMenuItem("Play", () => { })
			{
				Width = 200,
				Height = 40,

			};
			playButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(playButton);


			var debugButton = new UiMenuItem("Debug World", () => { })
			{
				Width = 200,
				Height = 40,

			};
			debugButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(debugButton);


			var optionsButton = new UiMenuItem("Options", () => { })
			{
				Width = 200,
				Height = 40,

			};
			optionsButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(optionsButton);


			var exitButton = new UiMenuItem("Exit Game", () => { })
			{
				Width = 200,
				Height = 40,

			};
			exitButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(exitButton);

			menuWrapper.Controls.Add(stackMenu);

			Gui.Controls.Add(menuWrapper);

			var logo = new UiElement()
			{
				ClassName = "TitleScreenLogo",
				Width = 450,
				Height = 150,
				Margin = new Thickness(500, 100, 100, 100)
			};
			Gui.Controls.Add(logo);

			Alex.IsMouseVisible = true;
		}


	}

	class TitleSkyBoxBackground
	{

	}
}
