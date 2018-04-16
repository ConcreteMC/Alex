using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.GameStates.Gui.Common;
using Alex.GameStates.Gui.Elements;

namespace Alex.GameStates.Gui.MainMenu
{
    public class OptionsState : GuiStateBase
    {
        private GuiStackContainer _stack;

        public OptionsState() : base()
        {
            Title = "Options";

            Gui.AddChild(new GuiBackButton());

            Gui.AddChild(_stack = new GuiStackContainer()
            {
                Y = Header.Height
            });

            _stack.AddChild(new GuiSlider()
            {
            });
            _stack.AddChild(new GuiSlider()
            {
                StepInterval = 10.0f
            });

        }
    }
}
