using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.Gui.Input.Listeners
{
    public class MouseListener : IMouseListener
    {

        public event EventHandler<MouseEventArgs> MouseDown;
        public event EventHandler<MouseEventArgs> MouseUp;
        
        public event EventHandler<MouseEventArgs> MouseMove;
        public event EventHandler<MouseEventArgs> MouseScroll;

        public Point Position { get; private set; }
        public MouseState CurrentState => _currentState;

        public int ClickThresholdMiliseconds { get; set; } = 1000;

        private GameTime _lastGameTime;
        private MouseState _currentState, _lastState;

        public MouseListener()
        {

        }

        public void Update(GameTime gameTime)
        {
            _lastGameTime = gameTime;
            _currentState = Mouse.GetState();

            Position = _currentState.Position;

            var moveDelta = _lastState.Position - _currentState.Position;
            var moveDistance = (moveDelta.X * moveDelta.X) + (moveDelta.Y * moveDelta.Y);
            if (moveDistance > 0)
            {
                var args = new MouseEventArgs(_lastState, _currentState, MouseButton.None);
                MouseMove?.Invoke(this, args);
            }

            CheckButton(s => s.LeftButton, MouseButton.Left);
            CheckButton(s => s.MiddleButton, MouseButton.Middle);
            CheckButton(s => s.RightButton, MouseButton.Right);
            CheckButton(s => s.XButton1, MouseButton.XButton1);
            CheckButton(s => s.XButton2, MouseButton.XButton2);

            _lastState = _currentState;
        }

        private void CheckButton(Func<MouseState, ButtonState> getButtonStateFunc, MouseButton button)
        {
            var currentState = getButtonStateFunc(_currentState);
            var lastState = getButtonStateFunc(_lastState);

            if (currentState == ButtonState.Released &&
                lastState == ButtonState.Pressed)
            {
                // Mouse Up
                var args = new MouseEventArgs(_lastState, _currentState, button);
                
                MouseUp?.Invoke(this, args);
            }
            else if (currentState == ButtonState.Pressed && lastState == ButtonState.Released)
            {
                var args = new MouseEventArgs(_lastState, _currentState, button);

                MouseDown?.Invoke(this, args);
            }
        }
    }
}
