using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Rendering
{
    public interface IGuiRenderer
    {
        void Init(GraphicsDevice graphics);

        BitmapFont Font { get; }

        IFontRenderer DefaultFont { get; }

        TextureSlice2D GetTexture(GuiTextures guiTexture);
        Texture2D GetTexture2D(GuiTextures guiTexture);


    }
}
