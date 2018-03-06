using System;
using Alex.Graphics.UI.Input.Listeners;

namespace Alex.Graphics.UI.Common
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
