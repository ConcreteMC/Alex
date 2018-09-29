using System;
using System.Linq;
using Alex.API.Gui.Elements.Layout;
using Microsoft.Xna.Framework;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiStackMenu : GuiStackContainer
    {
	    private bool _modern = true;

	    public bool ModernStyle
	    {
		    get => _modern;
		    set
		    {
			    _modern = value;
			    foreach (var child in AllChildren.OfType<GuiStackMenuItem>())
			    {
				    child.Modern = value;
			    }
		    }
	    }

        public GuiStackMenu()
        {
        }

        public void AddMenuItem(string label, Action action, bool enabled = true)
        {
            AddChild(new GuiStackMenuItem(label, action)
            {
				Enabled = enabled,
				Modern = ModernStyle
			});
        }

    }
}
