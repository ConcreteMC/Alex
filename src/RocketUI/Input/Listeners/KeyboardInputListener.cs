using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RocketUI.Input.Listeners
{
    public class KeyboardInputListener : InputListenerBase<KeyboardState, Keys>
    {
        public KeyboardInputListener() : base(PlayerIndex.One)
        {
            RegisterMap(GuiInputCommand.GuiNavigateAccept, Keys.Enter);
            RegisterMap(GuiInputCommand.GuiNavigateBack, Keys.Back);
            RegisterMap(GuiInputCommand.CursorClick, Keys.Space);

            RegisterMap(GuiInputCommand.GuiMenuNavigateLeft, Keys.Left);
            RegisterMap(GuiInputCommand.GuiMenuNavigateRight, Keys.Right);
            RegisterMap(GuiInputCommand.GuiMenuNavigateUp, Keys.Up);
            RegisterMap(GuiInputCommand.GuiMenuNavigateDown, Keys.Down);
            
            RegisterMap(GuiInputCommand.GuiNavigateExit, Keys.Escape);

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
