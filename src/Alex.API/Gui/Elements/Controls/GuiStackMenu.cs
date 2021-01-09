using System;
using System.Linq;
using Alex.API.Gui.Elements.Layout;

namespace Alex.API.Gui.Elements.Controls
{
    public class GuiStackMenu : GuiScrollableStackContainer
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

        public GuiStackMenuItem AddMenuItem(string label, Action action, bool enabled = true, bool isTranslationKey = false)
        {
	        GuiStackMenuItem item;
            AddChild(item = new GuiStackMenuItem(label, action, isTranslationKey)
            {
				Enabled = enabled,
				Modern = ModernStyle
			});

            return item;
        }

        public GuiStackMenuLabel AddLabel(string label, bool isTranslation = false)
        {
	        GuiStackMenuLabel element = new GuiStackMenuLabel(label, isTranslation);
	        AddChild(element);

	        return element;
        }
        
	    public void AddSpacer()
	    {
		    AddChild(new GuiStackMenuSpacer());
	    }
	}
}
