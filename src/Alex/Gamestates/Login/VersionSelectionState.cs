using System;
using Alex.API.Graphics;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gui;
using Alex.Networking.Java;
using Microsoft.Xna.Framework;
using MiNET.Net;
using RocketUI;

namespace Alex.Gamestates.Login
{
	public class VersionSelectionState : GuiMenuStateBase
	{
		private readonly GuiStackMenu _mainMenu;
		private readonly GuiImage _logo;
		private readonly GuiTextElement _textElement;
		private GuiPanoramaSkyBox _skyBox;
		private Action JavaConfirmed { get; }
		private Action<PlayerProfile> BedrockConfirmed { get; }
		public VersionSelectionState(GuiPanoramaSkyBox skyBox, Action onJavaConfirmed, Action<PlayerProfile> onBedrockConfirmed)
		{
			_skyBox = skyBox;
			JavaConfirmed = onJavaConfirmed;
			BedrockConfirmed = onBedrockConfirmed;
			
			Background = new GuiTexture2D(_skyBox, TextureRepeatMode.Stretch);

			_mainMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Width = 125,
				Anchor = Alignment.MiddleCenter,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				//BackgroundOverlay = new Color(Color.Black, 0.35f),
				ModernStyle =false,
			};

			_mainMenu.AddMenuItem($"Java - Version {JavaProtocol.FriendlyName}", JavaEditionButtonPressed);
			_mainMenu.AddMenuItem($"Bedrock - Version {McpeProtocolInfo.GameVersion}", BedrockEditionButtonPressed);

			_mainMenu.AddSpacer();
			
			_mainMenu.AddMenuItem($"Go Back", SinglePlayerButtonPressed);

			AddChild(_mainMenu);

			AddChild(_logo = new GuiImage(GuiTextures.AlexLogo)
			{
				Margin = new Thickness(0, 25, 0, 0),
				Anchor = Alignment.TopCenter
			});

			AddChild(_textElement = new GuiTextElement()
			{
				TextColor = TextColor.Yellow,

				Margin = new Thickness(0,64, 0, 0),
				Anchor = Alignment.TopCenter,

				Text = "Select the edition you want to play on...",
				Scale = 1f
			});
		}

		private void SinglePlayerButtonPressed()
		{
			Alex.GameStateManager.Back();
			/*Alex.GameStateManager.SetAndUpdateActiveState<TitleState>(state =>
			{
				state.EnableMultiplayer = false;
				return state;
			});*/
		}

		private void BedrockEditionButtonPressed()
		{
			BEDeviceCodeLoginState state = new BEDeviceCodeLoginState(_skyBox, BedrockConfirmed);
			Alex.GameStateManager.SetActiveState(state, true);
		}

		private void JavaEditionButtonPressed()
		{
			JavaLoginState loginState = new JavaLoginState(_skyBox, JavaConfirmed);
			Alex.GameStateManager.SetActiveState(loginState, true);
			//Alex.GameStateManager.SetActiveState(new JavaLoginState(), true);
		}

		private void MenuButtonClicked()
		{
			Alex.GameStateManager.SetActiveState<TitleState>();
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			_skyBox.Update(gameTime);
		}
		
		protected override void OnDraw(IRenderArgs args)
		{
			if (!_skyBox.Loaded)
			{
				_skyBox.Load(Alex.GuiRenderer);
			}

			_skyBox.Draw(args);
            
			base.OnDraw(args);
		}
	}
}
