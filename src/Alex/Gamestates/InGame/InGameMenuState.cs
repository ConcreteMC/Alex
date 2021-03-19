using System.Linq;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
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
	    
	    private readonly StackMenu _mainMenu;
	    private readonly StackContainer _playerList;
		public InGameMenuState()
        {
	        HeaderTitle.TranslationKey = "menu.game";
	        HeaderTitle.Anchor = Alignment.TopCenter;
	        HeaderTitle.Scale = 2f;
	        HeaderTitle.FontStyle = FontStyle.DropShadow;

			_mainMenu = new StackMenu()
			{
				Margin = new Thickness(15, 0, 15, 0),
				Padding = new Thickness(0, 50, 0, 0),
				Width = 125,
				Anchor = Alignment.FillY | Alignment.MinX,

				ChildAnchor = Alignment.CenterY | Alignment.FillX,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			};

	        _playerList = new ScrollableStackContainer()
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
	        
		/*	AddChild(new TextElement()
			{
				TranslationKey = "menu.game",
				Anchor = Alignment.TopCenter,
				Scale = 2f,
				FontStyle = FontStyle.DropShadow
			});*/

			_mainMenu.AddChild(new AlexButton("menu.returnToGame", OnReturnToGameButtonPressed, true));
			_mainMenu.AddChild(new AlexButton("menu.options", OnOptionsButtonPressed, true));
			_mainMenu.AddChild(new AlexButton("menu.returnToMenu", OnQuitButtonPressed, true));

			AddChild(_mainMenu);
			AddChild(_playerList);
        }

		private bool _didInitialization = false;
		protected override void OnShow()
		{
			if (!_didInitialization)
			{
				_didInitialization = true;

				if ( Alex.GameStateManager.TryGetState("play", out PlayingState s))
				{
					PlayerListItem[] players = s.World.PlayerList.Entries.Values.ToArray();

					for (var index = 0; index < players.Length; index++)
					{
						var p           = players[index];
						var displayName = p.Username;
						
						_playerList.AddChild(
							new PlayerListItemElement(displayName, p.IsJavaPlayer ? p.Ping : (int?)null)
							{
								BackgroundOverlay = (index % 2 == 0) ? new Color(Color.Black, 0.35f) :
									Color.Transparent,
								//MaxWidth = _playerList.MaxWidth
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
		        state.ParentState = ParentState;
		        Alex.GameStateManager.SetActiveState("options");
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
