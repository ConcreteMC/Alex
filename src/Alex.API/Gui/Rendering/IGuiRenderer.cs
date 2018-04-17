using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Gui.Rendering
{
    public interface IGuiRenderer
    {
        GuiScaledResolution ScaledResolution { get; set; }
        void Init(GraphicsDevice graphics);

        BitmapFont Font { get; }

        IFontRenderer DefaultFont { get; }

        TextureSlice2D GetTexture(GuiTextures guiTexture);
        Texture2D GetTexture2D(GuiTextures guiTexture);

        Vector2 Project(Vector2 point);
        Vector2 Unproject(Vector2 screen);
    }
}
