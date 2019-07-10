using Alex.API.Gui.Graphics;
using RocketUI;

namespace Alex.API.Gui.Elements
{
    public class GuiCrosshair : GuiElement
    {
        public GuiCrosshair()
        {
            Anchor = Alignment.MiddleCenter;
            Width = 15;
            Height = 15;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Crosshair);
        }
    }
}
