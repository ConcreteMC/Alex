using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Input.Listeners
{
    public class MouseInputListener : InputListenerBase<MouseState, MouseButton>, ICursorInputListener
    {
        public MouseInputListener(PlayerIndex playerIndex) : base(playerIndex)
        {
            RegisterMap(InputCommand.LeftClick, MouseButton.Left);
            RegisterMap(InputCommand.RightClick, MouseButton.Right);
            RegisterMap(InputCommand.MiddleClick, MouseButton.Middle);
            RegisterMap(InputCommand.HotBarSelectPrevious, MouseButton.ScrollDown);
            RegisterMap(InputCommand.HotBarSelectNext, MouseButton.ScrollUp);
        }
        
        private int _lastScroll;
        private int _scrollValue;

        private ButtonState _scrollUp, _scrollDown, _lastScrollUp, _lastScrollDown;

        protected override MouseState GetCurrentState()
        {
            _lastScroll = _scrollValue;
            _scrollValue = CurrentState.ScrollWheelValue;

            _lastScrollUp = _scrollUp;
            _lastScrollDown = _scrollDown;

            return Mouse.GetState();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _scrollUp = _scrollValue > _lastScroll ? ButtonState.Pressed : ButtonState.Released;
            _scrollDown = _scrollValue < _lastScroll ? ButtonState.Pressed : ButtonState.Released;
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
                
                // SPECIAL CASE
                case MouseButton.ScrollUp:
                    return state == CurrentState ? _scrollUp == buttonState : _lastScrollUp == buttonState;
                case MouseButton.ScrollDown:
                    return state == CurrentState ? _scrollDown == buttonState : _lastScrollDown == buttonState;
            }

            return false;
        }

        public Vector2 GetCursorPositionDelta()
        {
            return (CurrentState.Position.ToVector2() - PreviousState.Position.ToVector2());
        }

        public Vector2 GetCursorPosition()
        {
            return CurrentState.Position.ToVector2();
        }

	    public bool IsButtonDown(MouseButton button)
	    {
		    return CheckButtonState(CurrentState, button, ButtonState.Pressed);
	    }

	    public bool IsButtonUp(MouseButton button)
	    {
		    return CheckButtonState(CurrentState, button, ButtonState.Released);
	    }
	}
}
