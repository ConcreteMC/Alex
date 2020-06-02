using Alex.API.Graphics.Textures;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using RocketUI;

namespace Alex.API.Gui.Elements
{
    public class GuiImage : GuiElement
    {
        public GuiImage(GuiTextures texture, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            Background = texture;
            Background.RepeatMode = mode;
        }

        public GuiImage(NinePatchTexture2D background, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            Background = background;
            Background.RepeatMode = mode;
            Width = background.ClipBounds.Width;
            Height = background.ClipBounds.Height;
        }

        protected override void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
        {
            base.GetPreferredSize(out size, out minSize, out maxSize);
            if (Background.HasValue)
            {
                size = new Size(Background.Width, Background.Height);
                size = Size.Clamp(size, minSize, maxSize);
            }
        }
    }
}
