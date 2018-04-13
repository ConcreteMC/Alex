using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Rendering
{
    public interface IGuiRenderer
    {
        void Init(GraphicsDevice graphics);


        IFontRenderer DefaultFont { get; }

        NinePatchTexture2D GetTexture(GuiTextures guiTexture);


    }
}
