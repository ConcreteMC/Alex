using Alex.API.Graphics;
using Alex.API.Gui.Graphics;

namespace Alex.Gui.Elements
{
    public interface IGuiContext3DDrawable
    {
        void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer);
        void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer);
    }
}