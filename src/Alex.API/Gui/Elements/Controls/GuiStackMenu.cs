using System;
using Alex.API.Gui.Elements.Layout;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiStackMenu : GuiStackContainer
    {

        public GuiStackMenu()
        {
        }

        public void AddMenuItem(string label, Action action)
        {
            AddChild(new GuiStackMenuItem(label, action)
            {
            });
        }

    }
}
