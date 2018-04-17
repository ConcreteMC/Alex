using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiStackMenu : GuiStackContainer
    {

        public GuiStackMenu()
        {
	        DebugColor = Color.Red;
        }

        public void AddMenuItem(string label, Action action)
        {
            AddChild(new GuiStackMenuItem(label, action)
            {
				HorizontalAlignment = HorizontalAlignment.FillParent,
				VerticalAlignment = VerticalAlignment.Top,
            });
        }

    }
}
