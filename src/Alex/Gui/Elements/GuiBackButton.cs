using RocketUI;

namespace Alex.Gui.Elements
{
    public class GuiBackButton : Button
    {
        public GuiBackButton() : base("Back", () => Alex.Instance.GameStateManager.Back())
        {

        }
    }
}
