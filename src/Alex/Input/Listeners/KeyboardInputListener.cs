using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Alex.Input.Listeners
{
    public class KeyboardInputListener : InputListenerBase<KeyboardState, Keys>
    {
        public KeyboardInputListener() : base(PlayerIndex.One)
        {
            RegisterMap(InputCommand.MoveForwards, Keys.W);
            RegisterMap(InputCommand.MoveBackwards, Keys.S);
            RegisterMap(InputCommand.MoveLeft, Keys.A);
            RegisterMap(InputCommand.MoveRight, Keys.D);
            RegisterMap(InputCommand.MoveUp, Keys.Space);
            RegisterMap(InputCommand.MoveDown, Keys.LeftShift);

            RegisterMap(InputCommand.MoveSpeedIncrease, Keys.OemPlus);
            RegisterMap(InputCommand.MoveSpeedDecrease, Keys.OemMinus);
            RegisterMap(InputCommand.MoveSpeedReset, Keys.OemPeriod);

            RegisterMap(InputCommand.ToggleFog, Keys.F);
            RegisterMap(InputCommand.ToggleMenu, Keys.Escape);
            RegisterMap(InputCommand.ToggleDebugInfo, Keys.F3);
            RegisterMap(InputCommand.ToggleChat, Keys.T);

            RegisterMap(InputCommand.ToggleCamera, Keys.F5);
            RegisterMap(InputCommand.ToggleCameraFree, Keys.F8);
            RegisterMap(InputCommand.RebuildChunks, Keys.F9);
            RegisterMap(InputCommand.ToggleWireframe, Keys.F10);

            
            RegisterMap(InputCommand.HotBarSelect1, Keys.D1);
            RegisterMap(InputCommand.HotBarSelect2, Keys.D2);
            RegisterMap(InputCommand.HotBarSelect3, Keys.D3);
            RegisterMap(InputCommand.HotBarSelect4, Keys.D4);
            RegisterMap(InputCommand.HotBarSelect5, Keys.D5);
            RegisterMap(InputCommand.HotBarSelect6, Keys.D6);
            RegisterMap(InputCommand.HotBarSelect7, Keys.D7);
            RegisterMap(InputCommand.HotBarSelect8, Keys.D8);
            RegisterMap(InputCommand.HotBarSelect9, Keys.D9);

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
