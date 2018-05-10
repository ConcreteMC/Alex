using System.Drawing;
using RocketUI.Graphics.Textures;

namespace RocketUI.Elements
{
    public class GuiImage : VisualElement
    {
        public GuiImage(string textureResource, TextureRepeatMode mode = TextureRepeatMode.ScaleToFit)
        {
            Background = new GuiTexture2D(textureResource, mode);
        }

        public GuiImage(NinePatchTexture2D background, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            Background = new GuiTexture2D(background, mode);
            Width = background.ClipBounds.Width;
            Height = background.ClipBounds.Height;
        }

        protected override void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
        {
            base.GetPreferredSize(out size, out minSize, out maxSize);
            if (Background.HasValue && (Width == 0 && Height == 0))
            {
                size = new Size(Background.Width, Background.Height);
                size = Size.Clamp(size, minSize, maxSize);
            }
        }
    }
}
