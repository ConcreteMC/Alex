using Alex.API.Graphics;
using RocketUI;
using RocketUI.Elements;

namespace Alex.API.Gui.Elements
{
    public class GuiCrosshair : VisualElement
    {
        public GuiCrosshair()
        {
            Anchor = Anchor.MiddleCenter;
            Width = 15;
            Height = 15;
            Background = GuiTextures.Crosshair;
        }

    }
}
