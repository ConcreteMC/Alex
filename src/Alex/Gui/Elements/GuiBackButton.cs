using Alex.API.Gui.Elements.Controls;

namespace Alex.Gui.Elements
{
    public class GuiBackButton : GuiButton
    {
        public GuiBackButton() : base("Back", () => Alex.Instance.GameStateManager.Back())
        {

        }
    }
}
