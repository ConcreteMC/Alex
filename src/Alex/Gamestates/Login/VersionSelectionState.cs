using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Alex.Networking.Java;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.Login
{
	public class VersionSelectionState : GuiGameStateBase
	{
		private readonly GuiStackMenu _mainMenu;
		private readonly GuiImage _logo;
		private readonly GuiTextElement _textElement;
		public VersionSelectionState()
		{
			Background = new GuiTexture2D
			{
				TextureResource = GuiTextures.OptionsBackground,
				RepeatMode = TextureRepeatMode.Tile,
				Scale = new Vector2(2f, 2f),
			};
			BackgroundOverlay = new Color(Color.Black, 0.65f);

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
			_mainMenu.AddMenuItem($"Bedrock - Unavailable", BedrockEditionButtonPressed);

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
			Alex.GameStateManager.SetActiveState<TitleState>();
			/*Alex.GameStateManager.SetAndUpdateActiveState<TitleState>(state =>
			{
				state.EnableMultiplayer = false;
				return state;
			});*/
		}

		private void BedrockEditionButtonPressed()
		{
			
		}

		private void JavaEditionButtonPressed()
		{
			//Alex.GameStateManager.SetActiveState(new JavaLoginState(), true);
		}

		private void MenuButtonClicked()
		{
			Alex.GameStateManager.SetActiveState<TitleState>();
		}
	}
}
