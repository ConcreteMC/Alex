using System;
using System.Collections.Generic;
using System.Text;
using Alex.Input.Listeners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.Input.Listeners
{
    public class GamePadInputListener : InputListenerBase<GamePadState, Buttons>
    {
        private GamePadCapabilities _gamePadCapabilities;

        public GamePadInputListener(PlayerIndex playerIndex) : base(playerIndex)
        {
            RegisterMap(InputCommand.MoveForwardy, Buttons.LeftThumbstickUp);
            RegisterMap(InputCommand.MoveBackie, Buttons.LeftThumbstickDown);
            RegisterMap(InputCommand.MoveLeftie, Buttons.LeftThumbstickLeft);
            RegisterMap(InputCommand.MoveRightie, Buttons.LeftThumbstickRight);
            RegisterMap(InputCommand.MoveUppy, Buttons.A);
            RegisterMap(InputCommand.MoveDownie, Buttons.B);

            RegisterMap(InputCommand.MoveSpeedIncrease, Buttons.RightTrigger);
            RegisterMap(InputCommand.MoveSpeedDecrease, Buttons.LeftTrigger);
            RegisterMap(InputCommand.MoveSpeedReset, Buttons.LeftStick);

            RegisterMap(InputCommand.ToggleFog, Buttons.X);
            RegisterMap(InputCommand.ToggleMenu, Buttons.Start);
            RegisterMap(InputCommand.ToggleChat, Buttons.Back);

            RegisterMap(InputCommand.ToggleCamera, Buttons.Y);

            RegisterMap(InputCommand.HotBarSelectPrevious, Buttons.LeftShoulder);
            RegisterMap(InputCommand.HotBarSelectNext, Buttons.RightShoulder);
        }

        protected override GamePadState GetCurrentState()
        {
            return GamePad.GetState(PlayerIndex);
        }

        protected override bool IsButtonDown(GamePadState state, Buttons buttons)
        {
            return state.IsButtonDown(buttons);
        }

        protected override bool IsButtonUp(GamePadState state, Buttons buttons)
        {
            return state.IsButtonUp(buttons);
        }

        protected override void OnUpdate()
        {
            _gamePadCapabilities = GamePad.GetCapabilities(PlayerIndex);
        }
    }
}
