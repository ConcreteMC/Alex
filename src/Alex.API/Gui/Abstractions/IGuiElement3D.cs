using Alex.API.Gui.Graphics;

namespace Alex.API.Gui
{
    public interface IGuiElement3D : IGuiElement
    {

        void Draw3D(GuiRenderArgs renderArgs);
    }
}
