using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;

namespace Alex.Gamestates
{
    public class OptionsState : GameState
    {
        public OptionsState(Alex alex) : base(alex)
        {
        }

        protected override void OnLoad(RenderArgs args)
        {
            var stackPanel = new GuiStackContainer();

            var backButton = new GuiBeaconButton("Back", () =>
            {
                Alex.GameStateManager.Back();
            });

            stackPanel.AddChild(backButton);

            Gui = new GuiScreen(Alex);
            Gui.AddChild(stackPanel);
        }
    }
}
