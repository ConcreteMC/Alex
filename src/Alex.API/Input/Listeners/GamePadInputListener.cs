using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.API.Input.Listeners
{
    public class GamePadInputListener : InputListenerBase<GamePadState, Buttons>
    {
        private GamePadCapabilities _gamePadCapabilities;

        public GamePadInputListener(PlayerIndex playerIndex) : base(playerIndex)
        {
            RegisterMap(InputCommand.MoveForwards, Buttons.LeftThumbstickUp);
            RegisterMap(InputCommand.MoveBackwards, Buttons.LeftThumbstickDown);
            RegisterMap(InputCommand.MoveLeft, Buttons.LeftThumbstickLeft);
            RegisterMap(InputCommand.MoveRight, Buttons.LeftThumbstickRight);
            RegisterMap(InputCommand.MoveUp, Buttons.A);
            RegisterMap(InputCommand.MoveDown, Buttons.B);

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

        protected override void OnUpdate(GameTime gameTime)
        {
            _gamePadCapabilities = GamePad.GetCapabilities(PlayerIndex);
        }
    }
}
