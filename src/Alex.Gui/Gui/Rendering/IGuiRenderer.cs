﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui.Rendering
{
    public interface IGuiRenderer
    {
        void Init(GraphicsDevice graphics);


        SpriteFont DefaultFont { get; }

        Texture2D GetTexture(GuiTextures guiTexture);


    }
}