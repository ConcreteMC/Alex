using System.Linq;
using Alex.API.Graphics.Typography;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Alex.Gamestates.Common;
using Alex.Gamestates.MainMenu;
using Microsoft.Xna.Framework;
using NLog;
using RocketUI;

namespace Alex.Gamestates.InGame
{
    public class InGameMenuState : GuiInGameStateBase
    {
	    private static Logger Log = LogManager.GetCurrentClassLogger();
	    
	    private readonly GuiStackMenu _mainMenu;
	    private readonly GuiStackContainer _playerList;
		public InGameMenuState()
        {
	        HeaderTitle.TranslationKey = "menu.game";
	        HeaderTitle.Anchor = Alignment.TopCenter;
	        HeaderTitle.Scale = 2f;
	        HeaderTitle.FontStyle = FontStyle.DropShadow;

			_mainMenu = new GuiStackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			};

	        _playerList = new GuiScrollableStackContainer()
	        {
		        Margin = new Thickness(15, 0, 15, 0),
		        Padding = new Thickness(0, 0, 0, 0),
		        Width = 125,
				MinWidth = 125,
				Anchor = Alignment.FillRight,
		        ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			};
	        _playerList.Orientation = Orientation.Vertical;
	        
		/*	AddChild(new GuiTextElement()
			{
				TranslationKey = "menu.game",
				Anchor = Alignment.TopCenter,
				Scale = 2f,
				FontStyle = FontStyle.DropShadow
			});*/

			_mainMenu.AddMenuItem("menu.returnToGame", OnReturnToGameButtonPressed, isTranslationKey: true);
	        _mainMenu.AddMenuItem("menu.options", OnOptionsButtonPressed, isTranslationKey: true);
			_mainMenu.AddMenuItem("menu.returnToMenu", OnQuitButtonPressed, isTranslationKey: true);

			AddChild(_mainMenu);
			AddChild(_playerList);
        }

		private bool _didInitialization = false;
		protected override void OnShow()
		{
			if (!_didInitialization)
			{
				_didInitialization = true;
				//var previousState = Alex.GameStateManager.GetPreviousState();
				if (Alex.GameStateManager.TryGetState("play", out PlayingState s))
					//if (previousState is PlayingState s)
				{
					PlayerListItem[] players = s.World.PlayerList.Entries.Values.ToArray();

					for (var index = 0; index < players.Length; index++)
					{
						var p = players[index];

						_playerList.AddChild(
							new PlayerListItemElement(p.Username)
							{
								BackgroundOverlay = (index % 2 == 0) ? new Color(Color.Black, 0.35f) :
									Color.Transparent
							});
					}
				}
			}

			base.OnShow();
		}

		private void OnReturnToGameButtonPressed()
        {
            Alex.IsMouseVisible = false;
            Alex.GameStateManager.Back();
            Alex.GameStateManager.RemoveState("ingamemenu");
        }

        private void OnOptionsButtonPressed()
        {
	        if (Alex.GameStateManager.TryGetState("options", out OptionsState state))
	        {
		        Alex.GameStateManager.SetActiveState("options");
		        state.ParentState = ParentState;
	        }
        }

        private void OnQuitButtonPressed()
        {
	        if (!Alex.GameStateManager.SetActiveState("title"))
	        {
		        Log.Warn($"Could not go back to titlestate.");
	        }

            Alex.GameStateManager.RemoveState("serverMenu");
            Alex.GameStateManager.RemoveState("play");
        }

        protected override void OnHide()
        {
	        base.OnHide();
	        Alex.GameStateManager.RemoveState("ingamemenu");
        }
    }
}
