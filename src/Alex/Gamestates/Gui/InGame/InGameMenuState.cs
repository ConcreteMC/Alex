using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Utils;
using Alex.Entities;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.MainMenu;
using Alex.GameStates.Playing;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.GameStates.Gui.InGame
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

	        _playerList = new GuiStackContainer()
	        {
		        Margin = new Thickness(15, 0, 15, 0),
		        Padding = new Thickness(0, 0, 0, 0),
				MinWidth = 125,
				Anchor = Alignment.FillY | Alignment.MaxX,
		        ChildAnchor = Alignment.TopLeft,
				BackgroundOverlay = new Color(Color.Black, 0.35f)
			};
	        _playerList.Orientation = Orientation.Vertical;

	        var previousState = Alex.GameStateManager.GetPreviousState();
	        if (previousState is PlayingState s)
	        {
		        PlayerListItem[] players = s.World.PlayerList.Entries.Values.ToArray();
		        for (var index = 0; index < players.Length; index++)
		        {
			        var p = players[index];
			        _playerList.AddChild(new GuiTextElement()
			        {
				        Text = p.Username,
				        BackgroundOverlay = (index % 2 == 0) ? new Color(Color.Black, 0.35f) : Color.Transparent
					});
		        }
	        }
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

        private void OnReturnToGameButtonPressed()
        {
            Alex.IsMouseVisible = false;
            Alex.GameStateManager.Back();
            Alex.GameStateManager.RemoveState("ingamemenu");
        }

        private void OnOptionsButtonPressed()
        {
            Alex.GameStateManager.SetActiveState<OptionsState>();
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
    }
}
