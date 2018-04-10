using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Gamestates.Gui
{
    public class GuiRenderer : 
        IGuiRenderer
    {
        public SpriteFont DefaultFont => Alex.Font;
        public Texture2D GetTexture(GuiTextures guiTexture)
        {
            throw new NotImplementedException();
        }
    }
}
