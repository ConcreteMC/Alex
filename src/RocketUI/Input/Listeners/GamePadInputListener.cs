using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RocketUI.Input.Listeners
{
    public class GamePadInputListener : InputListenerBase<GamePadState, Buttons>
    {
        private GamePadCapabilities _gamePadCapabilities;

        public GamePadInputListener(PlayerIndex playerIndex) : base(playerIndex)
        {
            RegisterMap(GuiInputCommand.GuiNavigateAccept, Buttons.A);
            RegisterMap(GuiInputCommand.GuiNavigateBack, Buttons.B);
            RegisterMap(GuiInputCommand.CursorClick, Buttons.LeftStick);

            RegisterMap(GuiInputCommand.GuiMenuNavigateLeft, Buttons.LeftThumbstickLeft);
            RegisterMap(GuiInputCommand.GuiMenuNavigateRight, Buttons.LeftThumbstickRight);
            RegisterMap(GuiInputCommand.GuiMenuNavigateUp, Buttons.LeftThumbstickUp);
            RegisterMap(GuiInputCommand.GuiMenuNavigateDown, Buttons.LeftThumbstickDown);
            
            RegisterMap(GuiInputCommand.GuiNavigateExit, Buttons.Start);

        }

        protected override GamePadState GetCurrentState()
        {
            return GamePad.GetState(PlayerIndex, GamePadDeadZone.Circular);
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
