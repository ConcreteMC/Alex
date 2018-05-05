using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RocketUI.Input.Listeners;

namespace RocketUI.Input
{
    public class PlayerInputManager
    {
        public PlayerIndex PlayerIndex { get; }
        public InputType InputType { get; private set; }

        private List<IInputListener> InputListeners { get; } = new List<IInputListener>();

        public List<InputActionBinding> Bindings { get; } = new List<InputActionBinding>();

        public PlayerInputManager(PlayerIndex playerIndex, InputType inputType = InputType.GamePad)
        {
            PlayerIndex = playerIndex;
            InputType = inputType;

            AddListener(new GamePadInputListener(playerIndex));
        }

        public void AddListener(IInputListener listener)
        {
            InputListeners.Add(listener);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var inputListener in InputListeners.ToArray())
            {
                inputListener.Update(gameTime);
            }
        }

        public bool IsDown(string command)
        {
            return InputListeners.Any(l => l.IsDown(command));
        }

        public bool IsUp(string command)
        {
            return InputListeners.Any(l => l.IsUp(command));
        }

        public bool IsBeginPress(string command)
        {
            return InputListeners.Any(l => l.IsBeginPress(command));
        }

        public bool IsPressed(string command)
        {
            return InputListeners.Any(l => l.IsPressed(command));
        }


    }
}
