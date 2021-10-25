using Alex.Common.Gui.Elements;
using Alex.Common.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.MainMenu;
using Alex.Gui;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.InGame
{
    public class DisconnectedDialog : DialogBase
    {
	    private readonly Alex _game;

	    private static DisconnectedDialog _activeDialog = null;
	    public TextElement DisconnectedTextElement { get; private set; }
	    
	    private StackContainer Header { get; }
	    private StackContainer Body { get; }
	    public DisconnectedDialog(Alex game)
		{
			_game = game;
			Anchor = Alignment.Fill;
			ContentContainer.Anchor = Alignment.Fill;
			BackgroundOverlay = new Color(Color.Black, 0.65f);
			
		//	TitleTranslationKey = "multiplayer.disconnect.generic";

			ContentContainer.AddChild(Header = new StackContainer()
			{
				Height              = 32,
				Padding = new Thickness(5),
				Margin = new Thickness(5),

				Anchor = Alignment.TopFill,
				ChildAnchor = Alignment.MiddleCenter
			});
			
			Header.AddChild(new TextElement()
			{
				TranslationKey = "multiplayer.disconnect.generic",
				FontStyle = FontStyle.Bold | FontStyle.DropShadow,
				TextColor = (Color) TextColor.White,
				Anchor = Alignment.MiddleCenter
			});
			
			ContentContainer.AddChild(Body = new StackContainer()
			{
				Orientation = Orientation.Vertical,
				Anchor = Alignment.Fill,
				ChildAnchor = Alignment.MiddleCenter
			});
			
			Body.ChildAnchor = Alignment.MiddleCenter;
			Body.AddChild(DisconnectedTextElement = new TextElement()
			{
				TranslationKey = "disconnect.lost",
				TextColor = (Color) TextColor.Red,
				Anchor = Alignment.MiddleCenter
			});

			Container footer;
			Body.AddChild( footer = new Container()
			{
				Padding = new Thickness(10)
			});
			
			footer.AddChild(new AlexButton(MenuButtonClicked)
			{
				TranslationKey = "gui.toTitle",
				Anchor = Alignment.MiddleCenter,
			}.ApplyModernStyle(false));
		}

		private void MenuButtonClicked()
		{
			_game.GameStateManager.RemoveState("play");
			_game.GameStateManager.SetActiveState<TitleState>("title");
			GuiManager?.HideDialog(this);

			_game.IsMouseVisible = true;
		}

		/// <inheritdoc />
		public override void OnShow()
		{
			_game.IsMouseVisible = true;
			_activeDialog = this;
			base.OnShow();
		}

		/// <inheritdoc />
	    public override void OnClose()
	    {
		    _activeDialog = null;
		    base.OnClose();
	    }

		public void UpdateText(string reason, bool isTranslation = false)
		{
			if (isTranslation)
			{
				DisconnectedTextElement.TranslationKey = reason;
			}
			else
			{
				DisconnectedTextElement.Text = reason;
			}
		}
		
		public static void Show(Alex game, string reason, bool isTranslation = false)
		{
			var activeScreen = _activeDialog;

			if (activeScreen == null)
			{
				activeScreen = new DisconnectedDialog(game);
				game.GuiManager.ShowDialog(activeScreen);
			}
			
			activeScreen?.UpdateText(reason, isTranslation);
		}
    }
}
