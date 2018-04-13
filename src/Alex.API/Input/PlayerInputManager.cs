using System.Collections.Generic;
using System.Linq;
using Alex.API.Input.Listeners;
using Microsoft.Xna.Framework;

namespace Alex.API.Input
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

        public void Update()
        {
            foreach (var inputListener in InputListeners.ToArray())
            {
                inputListener.Update();
            }
        }

        public bool IsDown(InputCommand command)
        {
            return InputListeners.Any(l => l.IsDown(command));
        }

        public bool IsUp(InputCommand command)
        {
            return InputListeners.Any(l => l.IsUp(command));
        }

        public bool IsBeginPress(InputCommand command)
        {
            return InputListeners.Any(l => l.IsBeginPress(command));
        }

        public bool IsPressed(InputCommand command)
        {
            return InputListeners.Any(l => l.IsPressed(command));
        }


    }
}
