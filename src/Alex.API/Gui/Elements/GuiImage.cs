using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Graphics;
using Alex.API.Gui.Rendering;

namespace Alex.API.Gui.Elements
{
    public class GuiImage : GuiElement
    {
        public override int Height => Background == null ? 0 : Background.Bounds.Height;
        public override int Width => Background == null ? 0 : Background.Bounds.Width;

        public GuiImage(GuiTextures texture)
        {
            DefaultBackgroundTexture = texture;
        }

        public GuiImage(NinePatchTexture2D background)
        {
            Background = background;
        }
    }
}
