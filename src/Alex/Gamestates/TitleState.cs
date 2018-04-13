using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Rendering;
using Alex.Gamestates.Gui;
using Alex.Worlds;
using Alex.Worlds.Generators;

namespace Alex.Gamestates
{
	public class TitleState : GameState
	{
		private GuiDebugInfo _debugInfo;

		public TitleState(Alex alex) : base(alex)
		{
			Gui = new GuiScreen(Alex)
			{
				DefaultBackgroundTexture = GuiTextures.TitleScreenBackground
			};
			var stackMenu = new GuiStackMenu()
			{
				LayoutOffsetX = 25,
				Width = 125,
				VerticalAlignment = VerticalAlignment.Center,

				VerticalContentAlignment = VerticalAlignment.Top,
				HorizontalContentAlignment = HorizontalAlignment.Stretch
			};

			stackMenu.AddMenuItem("Multiplayer", () =>
			{
				//TODO: Switch to multiplayer serverlist (maybe choose PE or Java?)
				Alex.ConnectToServer();
			});

			stackMenu.AddMenuItem("Debug Blockstates", DebugWorldButtonActivated);
			stackMenu.AddMenuItem("Debug Flatland", DebugFlatland);
			stackMenu.AddMenuItem("Debug Anvil", DebugAnvil);

			stackMenu.AddMenuItem("Options", () => { Alex.GameStateManager.SetActiveState("options"); });
			stackMenu.AddMenuItem("Exit Game", () => { Alex.Exit(); });

			Gui.AddChild(stackMenu);

			Gui.AddChild(new GuiImage(GuiTextures.AlexLogo)
			{
				LayoutOffsetX = 175,
				LayoutOffsetY = 25
			});

			_debugInfo = new GuiDebugInfo(alex);
			_debugInfo.AddDebugRight(() => $"Cursor Position: {alex.InputManager.CursorInputListener.GetCursorPosition()} / {alex.GuiManager.FocusManager.CursorPosition}");
			_debugInfo.AddDebugRight(() => $"Cursor Delta: {alex.InputManager.CursorInputListener.GetCursorPositionDelta()}");

		}

		protected override void OnLoad(RenderArgs args)
		{

			//var logo = new UiElement()
			//{
			//	ClassName = "TitleScreenLogo",
			//};
			//Gui.AddChild(logo);

			Alex.IsMouseVisible = true;
		}

		protected override void OnShow()
		{
			base.OnShow();
			Alex.GuiManager.AddScreen(_debugInfo);
		}

		protected override void OnHide()
		{
			Alex.GuiManager.RemoveScreen(_debugInfo);
			base.OnHide();
		}

		private void Debug(IWorldGenerator generator)
		{
			Alex.IsMultiplayer = false;

			Alex.IsMouseVisible = false;

			generator.Initialize();
			var debugProvider = new SPWorldProvider(Alex, generator);
			Alex.LoadWorld(debugProvider, debugProvider.Network);
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
