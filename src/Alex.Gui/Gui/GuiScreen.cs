using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Gui
{
    public class GuiScreen
    {
        public Viewport Viewport { get; set; }

        private UiScaledResolution ScaledResolution { get; }



        public GuiScreen(Game game)
        {
            ScaledResolution = new UiScaledResolution(game);
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {

        }

    }
}
