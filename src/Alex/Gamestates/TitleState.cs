using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Alex.API.World;
using Alex.Gamestates.Playing;
using Alex.Graphics;
using Alex.Graphics.UI;
using Alex.Graphics.UI.Common;
using Alex.Graphics.UI.Controls.Menu;
using Alex.Graphics.UI.Layout;
using Alex.Utils;
using Alex.Worlds;
using Alex.Worlds.Generators;
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
				ClassName = "TitleScreenMenuPanel"
			};
			var stackMenu = new UiMenu()
			{
				ClassName = "TitleScreenMenu"
			};

			stackMenu.AddMenuItem("Play", () => { });
			stackMenu.AddMenuItem("Debug World", DebugWorldButtonActivated);
			stackMenu.AddMenuItem("Options", () => { Alex.GameStateManager.SetActiveState("options"); });
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

		private void DebugWorldButtonActivated()
		{
			Alex.IsMouseVisible = false;

			IWorldGenerator generator;
			if (Alex.GameSettings.UseBuiltinGenerator || (string.IsNullOrWhiteSpace(Alex.GameSettings.Anvil) ||
			                                              !File.Exists(Path.Combine(Alex.GameSettings.Anvil, "level.dat"))))
			{
				generator = new OverworldGenerator();
			}
			else
			{
				generator = new AnvilWorldProvider(Alex.GameSettings.Anvil)
				{
					MissingChunkProvider = new VoidWorldGenerator()
				};
			}

			generator.Initialize();

			LoadWorld(new SPWorldProvider(Alex, generator));
		}

		private void LoadWorld(WorldProvider worldProvider)
		{

			LoadingWorldState loadingScreen =
				new LoadingWorldState(Alex, TextureUtils.ImageToTexture2D(Alex.GraphicsDevice, Resources.mcbg));
			Alex.GameStateManager.AddState("loading", loadingScreen);
			Alex.GameStateManager.SetActiveState("loading");

			worldProvider.Load(loadingScreen.UpdateProgress).ContinueWith(task =>
			{
				PlayingState playState = new PlayingState(Alex, Graphics, worldProvider);
				Alex.GameStateManager.AddState("play", playState);
				Alex.GameStateManager.SetActiveState("play");

				Alex.GameStateManager.RemoveState("loading");
			});
		}

	}

	class TitleSkyBoxBackground
	{

	}
}
