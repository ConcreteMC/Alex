using System;
using System.Collections.Generic;
using System.Text;
using Alex.API.Gui.Attributes;

namespace Alex.API.Gui
{
    public interface IFocusableElement : IGuiElement
    {
        [DebuggerVisible] bool Focused { get; set; }
    }
}
