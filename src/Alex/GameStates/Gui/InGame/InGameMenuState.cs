using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.MainMenu;
using RocketUI;
using RocketUI.Elements.Controls;

namespace Alex.GameStates.Gui.InGame
{
    public class InGameMenuState : GuiInGameStateBase
    {

        public InGameMenuState()
        {
            BodyMinWidth = 200;
            TitleTranslationKey = "menu.game";

            AddGuiRow(new Button(OnReturnToGameButtonPressed)
            {
                TranslationKey = "menu.returnToGame",
                Margin = new Thickness(0, 3, 0, 3),
            });

            var r = AddGuiRow(new Button(OnOptionsButtonPressed)
            {
                TranslationKey = "menu.options",
                Margin = new Thickness(0, 20, 3, 3),
            }, 
                      new Button(OnShareToLanButtonPressed)
            {
                TranslationKey = "menu.shareToLan",
                Margin = new Thickness(3, 20, 0, 3),
                Enabled = false
            });

            AddGuiRow(new Button(OnQuitButtonPressed)
            {
                TranslationKey = "menu.returnToMenu",
                Margin = new Thickness(0, 3, 0, 3),
            });
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

        private void OnShareToLanButtonPressed()
        {
            
        }

        private void OnQuitButtonPressed()
        {
            Alex.GameStateManager.SetActiveState<TitleState>();

            Alex.GameStateManager.RemoveState("serverMenu");
            Alex.GameStateManager.RemoveState("play");
        }
    }
}
