using System.Linq;
using Alex.Common.Gui.Elements;
using Alex.Common.Gui.Graphics;
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
	        HeaderTitle.IsVisible = false;

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

	        _mainMenu.AddChild(new AlexButton("menu.returnToGame", OnReturnToGameButtonPressed, true));
			_mainMenu.AddChild(new AlexButton("menu.options", OnOptionsButtonPressed, true));
			_mainMenu.AddChild(new AlexButton("menu.returnToMenu", OnQuitButtonPressed, true));

			AddChild(_mainMenu);
			AddChild(_playerList);
			
			AddChild(new Image(AlexGuiTextures.AlexLogo)
			{
				Margin = new Thickness(0, 25, 0, 0),
				Anchor = Alignment.TopCenter
			});
        }

		private bool _didInitialization = false;
		protected override void OnShow()
		{
			if (!_didInitialization)
			{
				_didInitialization = true;

				if ( Alex.GameStateManager.TryGetState("play", out PlayingState s))
				{
					foreach (var p in s.World.PlayerList)
					{
						var element = new PlayerListItemElement(p);

						_playerList.AddChild(element);
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
	        OptionsState state = new OptionsState(null);
	       //state.ParentState = ParentState;
	        
	        Alex.GameStateManager.SetActiveState(state, true);
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
