using System;
using System.Collections.Generic;
using System.Text;
using Alex.Gui.Input.Listeners;

namespace Alex.Gui.Common
{
    public interface IHoverable
    {
        event EventHandler<MouseEventArgs> MouseEnter;
        event EventHandler<MouseEventArgs> MouseMove; 
        event EventHandler<MouseEventArgs> MouseLeave;

        bool IsMouseOver { get; }


        void InvokeMouseEnter(MouseEventArgs args);
        void InvokeMouseMove(MouseEventArgs args);
        void InvokeMouseLeave(MouseEventArgs args);
    }
}
