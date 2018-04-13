using Alex.API.Gui.Rendering;

namespace Alex.API.Gui.Elements
{
    public class GuiCrosshair : GuiElement
    {
        public GuiCrosshair()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;
            Width = 15;
            Height = 15;
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            Background = renderer.GetTexture(GuiTextures.Crosshair);
        }
    }
}
