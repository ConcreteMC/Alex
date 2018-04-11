using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Alex.Input.Listeners
{
    public abstract class InputListenerBase<TState, TButtons> : IInputListener
    {
        public PlayerIndex PlayerIndex { get; }

        private readonly IDictionary<InputCommand, TButtons> _buttonMap = new Dictionary<InputCommand, TButtons>();

        protected TState PreviousState, CurrentState;

        protected abstract TState GetCurrentState();

        protected abstract bool IsButtonDown(TState state, TButtons buttons);
        protected abstract bool IsButtonUp(TState state, TButtons buttons);

        protected InputListenerBase(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public void Update()
        {
            PreviousState = CurrentState;
            CurrentState = GetCurrentState();

            OnUpdate();
        }

        protected virtual void OnUpdate()
        {

        }

        public void RegisterMap(InputCommand command, TButtons buttons)
        {
            _buttonMap.Add(command, buttons);
        }

        public bool IsDown(InputCommand command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonDown(CurrentState, buttons));
        }

        public bool IsUp(InputCommand command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonUp(CurrentState, buttons));
        }
        
        public bool IsBeginPress(InputCommand command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonDown(CurrentState, buttons) && IsButtonUp(PreviousState, buttons));
        }

        public bool IsPressed(InputCommand command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonUp(CurrentState, buttons) && IsButtonDown(PreviousState, buttons));
        }
        
        private bool TryGetButtons(InputCommand command, out TButtons buttons)
        {
            return _buttonMap.TryGetValue(command, out buttons);
        }
    }
}
