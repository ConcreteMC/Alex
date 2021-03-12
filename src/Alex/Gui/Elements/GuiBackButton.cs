using Alex.API.Gui.Elements;
using RocketUI;

namespace Alex.Gui.Elements
{
    public class GuiBackButton : AlexButton
    {
        public GuiBackButton() : base("Back", () => Alex.Instance.GameStateManager.Back())
        {

        }
    }
}
