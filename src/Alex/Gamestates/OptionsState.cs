using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Controls;
using Alex.Graphics.UI.Layout;

namespace Alex.Gamestates
{
    public class OptionsState : GameState
    {
        public OptionsState(Alex alex) : base(alex)
        {
        }

        protected override void OnLoad(RenderArgs args)
        {
            var stackPanel = new UiStackPanel();

            var backButton = new UiButton("Back", () =>
            {
                Alex.GameStateManager.SetActiveState("title");
            });

            var toggle1 = new UiToggleButton();

            stackPanel.AddChild(backButton);
            stackPanel.AddChild(toggle1);

            Gui.AddChild(stackPanel);
        }
    }
}
