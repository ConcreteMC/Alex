using System;
using System.Linq;
using RocketUI.Elements.Layout;

namespace Alex.API.Gui.Elements.Controls.Menus
{
    public class GuiMCStackMenu : GuiStackContainer
    {
	    private bool _modern = false;

	    public bool ModernStyle
	    {
		    get => _modern;
		    set
		    {
			    _modern = value;
			    foreach (var child in AllChildren.OfType<GuiMCStackMenuItem>())
			    {
				    child.Modern = value;
			    }
		    }
	    }

        public GuiMCStackMenu()
        {
        }

        public void AddMenuItem(string label, Action action, bool enabled = true)
        {
            AddChild(new GuiMCStackMenuItem(label, action)
            {
				Enabled = enabled,
				Disabled = !enabled,
				Modern = ModernStyle
			});
        }

    }
}
