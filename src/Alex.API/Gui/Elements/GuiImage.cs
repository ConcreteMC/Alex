using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Rendering;

namespace Alex.API.Gui.Elements
{
    public class GuiImage : GuiElement
    {
        public override int Height => Background == null ? 0 : Background.ClipBounds.Height;
        public override int Width => Background == null ? 0 : Background.ClipBounds.Width;

        public GuiImage(GuiTextures texture, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            DefaultBackgroundTexture = texture;
            BackgroundRepeatMode = mode;
        }

        public GuiImage(NinePatchTexture2D background, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            Background = background;
            BackgroundRepeatMode = mode;
        }
    }
}
