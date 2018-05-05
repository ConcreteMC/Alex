using Alex.API.Gui.Elements.Controls;
using RocketUI.Elements.Controls;

namespace Alex.Gui.Elements
{
    public class GuiBackButton : MCButton
    {
        public GuiBackButton() : base("Back", () => Alex.Instance.GameStateManager.Back())
        {

        }
    }
}
