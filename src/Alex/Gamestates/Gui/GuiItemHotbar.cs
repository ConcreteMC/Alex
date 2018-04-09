using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.Gui;
using Alex.Graphics.Gui.Rendering;

namespace Alex.Gamestates.Gui
{
    public class GuiItemHotbar : GuiElementGroup
    {

        public GuiItemHotbar()
        {
            Width = GuiScalar.FromAbsolute(180);
            Height = GuiScalar.FromAbsolute(20);
        }

    }
}
