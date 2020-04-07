using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Input.Listeners
{
    public class KeyboardInputListener : InputListenerBase<KeyboardState, Keys>
    {
        public static EventHandler<KeyboardInputListener> InstanceCreated;
        
        public KeyboardInputListener() : base(PlayerIndex.One)
        {
            InstanceCreated?.Invoke(this, this);
		}

        protected override KeyboardState GetCurrentState()
        {
            return Keyboard.GetState();
        }

        protected override bool IsButtonDown(KeyboardState state, Keys buttons)
        {
            return state.IsKeyDown(buttons);
        }

        protected override bool IsButtonUp(KeyboardState state, Keys buttons)
        {
            return state.IsKeyUp(buttons);
        }
    }
}
