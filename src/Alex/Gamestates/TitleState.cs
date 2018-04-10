using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
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
using Alex.Worlds.Java;
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

			stackMenu.AddMenuItem("Multiplayer", () =>
			{
				
			});

			stackMenu.AddMenuItem("Debug Blockstates", DebugWorldButtonActivated);
			stackMenu.AddMenuItem("Debug Flatland", DebugFlatland);
			stackMenu.AddMenuItem("Debug Anvil", DebugAnvil);

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

		private void Debug(IWorldGenerator generator)
		{
			Alex.IsMouseVisible = false;

			generator.Initialize();

			Alex.LoadWorld(new SPWorldProvider(Alex, generator));
		}

		private void DebugFlatland()
		{
			Debug(new FlatlandGenerator());
		}

		private void DebugAnvil()
		{
			Debug(new AnvilWorldProvider(Alex.GameSettings.Anvil)
			{
				MissingChunkProvider = new VoidWorldGenerator()
			});
		}

		private void DebugWorldButtonActivated()
		{
			Debug(new DebugWorldGenerator());
		}
	}

	class TitleSkyBoxBackground
	{

	}
}
