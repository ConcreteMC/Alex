using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Rendering
{
    public interface IGuiRenderer
    {
        void Init(GraphicsDevice graphics);


        SpriteFont DefaultFont { get; }

        NinePatchTexture2D GetTexture(GuiTextures guiTexture);
        Texture2D GetTexture2D(GuiTextures guiTexture);


    }
}
