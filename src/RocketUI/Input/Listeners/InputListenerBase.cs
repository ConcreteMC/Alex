using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RocketUI.Input.Listeners
{
    public abstract class InputListenerBase<TState, TButtons> : IInputListener
    {
        public PlayerIndex PlayerIndex { get; }

        private readonly IDictionary<string, TButtons> _buttonMap = new Dictionary<string, TButtons>();

        protected TState PreviousState, CurrentState;

        protected abstract TState GetCurrentState();

        protected abstract bool IsButtonDown(TState state, TButtons buttons);
        protected abstract bool IsButtonUp(TState state, TButtons buttons);

        protected InputListenerBase(PlayerIndex playerIndex)
        {
            PlayerIndex = playerIndex;
        }

        public void Update(GameTime gameTime)
        {
            PreviousState = CurrentState;
            CurrentState = GetCurrentState();

            OnUpdate(gameTime);
        }

        protected virtual void OnUpdate(GameTime gameTime)
        {

        }

        public void RegisterMap(string command, TButtons buttons)
        {
            _buttonMap.Add(command, buttons);
        }

        public bool IsDown(string command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonDown(CurrentState, buttons));
        }

        public bool IsUp(string command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonUp(CurrentState, buttons));
        }
        
        public bool IsBeginPress(string command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonDown(CurrentState, buttons) && IsButtonUp(PreviousState, buttons));
        }

        public bool IsPressed(string command)
        {
            return (TryGetButtons(command, out var buttons) && IsButtonUp(CurrentState, buttons) && IsButtonDown(PreviousState, buttons));
        }
        
        private bool TryGetButtons(string command, out TButtons buttons)
        {
            return _buttonMap.TryGetValue(command, out buttons);
        }
    }
}
