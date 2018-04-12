using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui.Rendering;
using Alex.Graphics.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui
{
    public class GuiScreen : GuiElement
    {
        protected Game Game { get; }
        
        public GuiScreen(Game game)
        {
            Game = game;
        }

        public void UpdateSize(int width, int height)
        {
            Width = width;
            Height = height;

            UpdateLayout();
        }
    }
}
