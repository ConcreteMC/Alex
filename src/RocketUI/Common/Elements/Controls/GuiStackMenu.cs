using System;
using System.Linq;
using RocketUI.Elements.Layout;

namespace RocketUI.Elements.Controls
{
    public class GuiStackMenu : GuiStackContainer
    {
        //private bool _modern = false;

        //public bool ModernStyle
        //{
        //    get => _modern;
        //    set
        //    {
        //        _modern = value;
        //        foreach (var child in AllChildren.OfType<GuiStackMenuItem>())
        //        {
        //            child.Modern = value;
        //        }
        //    }
        //}

        public GuiStackMenu()
        {

        }
        
        public void AddMenuItem(string label, Action action, bool enabled = true)
        {
            AddChild(new StackMenuItem(label, action)
            {
                Enabled  = enabled,
                //Disabled = !enabled,
                //Modern   = ModernStyle
            });
        }

    }
}
