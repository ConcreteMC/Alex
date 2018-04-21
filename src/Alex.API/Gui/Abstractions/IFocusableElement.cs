using System;
using System.Collections.Generic;
using System.Text;

namespace Alex.API.Gui
{
    public interface IFocusableElement : IGuiElement
    {
        bool Focused { get; set; }
    }
}
