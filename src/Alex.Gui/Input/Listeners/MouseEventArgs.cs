using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gui.Input.Listeners
{
    public class MouseEventArgs : EventArgs
    {
        public MouseState PreviousState { get; }
        public MouseState CurrentState { get; }

        public MouseButton Button { get; }
        public Point Position { get; }

        public int ScrollWheelValue { get; }
        public int ScrollWheelDelta { get; }

        public MouseEventArgs(MouseState previousState, MouseState currentState, MouseButton buttons)
        {
            PreviousState = previousState;
            CurrentState = currentState;
            Button = buttons;

            Position = currentState.Position;
            ScrollWheelValue = currentState.ScrollWheelValue;
            ScrollWheelDelta = previousState.ScrollWheelValue - currentState.ScrollWheelValue;
        }

    }
}
