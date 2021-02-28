using Alex.API.Graphics;
using Alex.API.Gui.Graphics;
using RocketUI;

namespace Alex.Gui.Elements.Context3D
{
    public interface IGuiContext3DDrawable
    {
        void UpdateContext3D(IUpdateArgs args, IGuiRenderer guiRenderer);
        void DrawContext3D(IRenderArgs args, IGuiRenderer guiRenderer);
    }
}