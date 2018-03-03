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
	public class TitleState : Gamestate
	{
		private ResourceManager _resourceManager;

		public TitleState(Alex game) : base(game)
		{
			_resourceManager = game.Resources;
		}

		public override void Init(RenderArgs args)
		{
			var bgTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.mcbg);
			var logoTexture = TextureUtils.ImageToTexture2D(args.GraphicsDevice, Resources.logo);

			Gui.Root.BackgroundImage = bgTexture;
			//Gui.Root.HorizontalContentAlignment = HorizontalAlignment.Left;

			var menuWrapper = new UiPanel()
			{
				Width = 250,
				BackgroundColor = new Color(Color.Black, 0.2f),
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
				BackgroundColor = Color.Gray,

			};
			playButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(playButton);


			var debugButton = new UiMenuItem("Debug World", () => { })
			{
				Width = 200,
				Height = 40,
				BackgroundColor = Color.Gray,

			};
			debugButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(debugButton);


			var optionsButton = new UiMenuItem("Options", () => { })
			{
				Width = 200,
				Height = 40,
				BackgroundColor = Color.Gray,

			};
			optionsButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(optionsButton);


			var exitButton = new UiMenuItem("Exit Game", () => { })
			{
				Width = 200,
				Height = 40,
				BackgroundColor = Color.Gray,

			};
			exitButton.Label.Font = Alex.Font;
			stackMenu.Controls.Add(exitButton);

			menuWrapper.Controls.Add(stackMenu);

			Gui.Root.Controls.Add(menuWrapper);

			var logo = new UiElement()
			{
				BackgroundColor = new Color(Color.Black, 0.8f),
				BackgroundImage = logoTexture,
				Width = 450,
				Height = 150,
				Margin = new Thickness(500, 100, 100, 100)
			};
			Gui.Root.Controls.Add(logo);

			Game.IsMouseVisible = true;
		}


	}

	class TitleSkyBoxBackground
	{

	}
}
