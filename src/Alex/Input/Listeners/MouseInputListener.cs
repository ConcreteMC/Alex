using System;
using System.Collections.Generic;
using System.Text;
using Alex.Graphics.UI.Input.Listeners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.Input.Listeners
{
    public class MouseInputListener : InputListenerBase<MouseState, MouseButton>
    {
        public MouseInputListener(PlayerIndex playerIndex) : base(playerIndex)
        {
        }

        protected override MouseState GetCurrentState()
        {
            return Mouse.GetState();
        }

        protected override bool IsButtonDown(MouseState state, MouseButton buttons)
        {
            return CheckButtonState(state, buttons, ButtonState.Pressed);
        }

        protected override bool IsButtonUp(MouseState state, MouseButton buttons)
        {
            return CheckButtonState(state, buttons, ButtonState.Released);
        }

        private bool CheckButtonState(MouseState state, MouseButton buttons, ButtonState buttonState)
        {
            switch (buttons)
            {
                case MouseButton.Left:
                    return state.LeftButton == buttonState;
                case MouseButton.Middle:
                    return state.MiddleButton == buttonState;
                case MouseButton.Right:
                    return state.RightButton == buttonState;
                case MouseButton.XButton1:
                    return state.XButton1 == buttonState;
                case MouseButton.XButton2:
                    return state.XButton2 == buttonState;
            }

            return false;
        }
    }
}
