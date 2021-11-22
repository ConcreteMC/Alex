using System;
using Alex.Common.GameStates;
using Alex.Common.Gui.Elements;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Worlds.Multiplayer.Java;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gamestates.Login
{
	public class JavaProviderSelectionState : GuiMenuStateBase
	{
		private readonly JavaServerType _serverType;

		private readonly GuiPanoramaSkyBox _backgroundSkyBox;
		private readonly Func<IGameState> _microsoft;
		private readonly Func<IGameState> _mojang;

		public JavaProviderSelectionState(JavaServerType serverType, GuiPanoramaSkyBox skyBox, Func<IGameState> microsoft, Func<IGameState> mojang)
		{
			Title = "Minecraft Login";
			_serverType = serverType;

			_backgroundSkyBox = skyBox;
			_microsoft = microsoft;
			_mojang = mojang;
			Background = new GuiTexture2D(_backgroundSkyBox, TextureRepeatMode.Stretch);
			BackgroundOverlay = Color.Transparent;

			base.HeaderTitle.Anchor = Alignment.MiddleCenter;
			base.HeaderTitle.FontStyle = FontStyle.Bold | FontStyle.DropShadow;
			Footer.ChildAnchor = Alignment.MiddleCenter;
			TextElement t;

			Footer.AddChild(
				t = new TextElement()
				{
					Text = "We are NOT in anyway or form affiliated with Mojang/Minecraft or Microsoft!",
					TextColor = (Color)TextColor.Yellow,
					Scale = 1f,
					FontStyle = FontStyle.DropShadow,
					Anchor = Alignment.MiddleCenter
				});

			TextElement info;

			Footer.AddChild(
				info = new TextElement()
				{
					Text = "We will never collect/store or do anything with your data.",
					TextColor = (Color)TextColor.Yellow,
					Scale = 0.8f,
					FontStyle = FontStyle.DropShadow,
					Anchor = Alignment.MiddleCenter,
					Padding = new Thickness(0, 5, 0, 0)
				});

			Body.BackgroundOverlay = new Color(Color.Black, 0.5f);
			Body.ChildAnchor = Alignment.MiddleCenter;

			Body.AddChild( new TextElement()
			{
				TextColor = (Color) TextColor.Cyan,
				Text = "Please choose your account type:",
				FontStyle = FontStyle.Italic,
				Scale = 1.1f
			});

			Body.AddRow(
				new AlexButton(MojangLoginButtonPressed)
				{
					AccessKey = Keys.Enter,
					Text = $"Login with Mojang account",
					Margin = new Thickness(5),
					Width = 100,
					Enabled = true,
				}.ApplyModernStyle(false),
				new AlexButton(MicrostLoginButtonPressed)
				{
					AccessKey = Keys.Enter,
					Text = $"Login with Microsoft account",
					Margin = new Thickness(5),
					Width = 100,
					Enabled = true,
				}.ApplyModernStyle(false));

			var buttonRow = AddGuiRow(
				new AlexButton(OnCancelButtonPressed)
				{
					AccessKey = Keys.Escape, TranslationKey = "gui.cancel", Margin = new Thickness(5), Width = 100
				}.ApplyModernStyle(false));

			buttonRow.ChildAnchor = Alignment.MiddleCenter;
		}

		private void OnCancelButtonPressed()
		{
			Alex.GameStateManager.Back();
		}

		private void Authenticate(IGameState state)
		{
			Alex.GameStateManager.SetActiveState(state);
		}
		
		private void MicrostLoginButtonPressed()
		{
			var state = _microsoft?.Invoke();
			Authenticate(state);
		}

		private void MojangLoginButtonPressed()
		{
			var state = _mojang?.Invoke();
			Authenticate(state);
		}
	}
}