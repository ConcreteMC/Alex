using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Graphics.Textures;
using Alex.API.Gui.Graphics;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements
{
    public class GuiImage : GuiElement
    {
        public GuiImage(GuiTextures texture, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            DefaultBackgroundTexture = texture;
            BackgroundRepeatMode = mode;
        }

        public GuiImage(NinePatchTexture2D background, TextureRepeatMode mode = TextureRepeatMode.Stretch)
        {
            Background = background;
            BackgroundRepeatMode = mode;
            Width = background.ClipBounds.Width;
            Height = background.ClipBounds.Height;
        }

        protected override void GetPreferredSize(out Size size, out Size minSize, out Size maxSize)
        {
            base.GetPreferredSize(out size, out minSize, out maxSize);
            if (Background != null)
            {
                size = new Size(Background.ClipBounds.Width, Background.ClipBounds.Height);
            }
        }
    }
}
