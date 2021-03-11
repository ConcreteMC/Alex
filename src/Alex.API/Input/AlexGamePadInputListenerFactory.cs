using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Input.Listeners;

namespace Alex.API.Input
{
    public class AlexGamePadInputListenerFactory : AlexInputListenerFactoryBase<GamePadInputListener>
    {
        public AlexGamePadInputListenerFactory() { }
        
        protected override GamePadInputListener CreateInstance(PlayerIndex playerIndex)
            => new GamePadInputListener(playerIndex);

        protected override void RegisterMaps(GamePadInputListener l)
        {
            l.ClearMap();

            l.RegisterMap(AlexInputCommand.MoveForwards, Buttons.LeftThumbstickUp);
            l.RegisterMap(AlexInputCommand.MoveBackwards, Buttons.LeftThumbstickDown);
            l.RegisterMap(AlexInputCommand.MoveLeft, Buttons.LeftThumbstickLeft);
            l.RegisterMap(AlexInputCommand.MoveRight, Buttons.LeftThumbstickRight);
            //l.RegisterMap(InputCommand.MoveUp, Buttons.A);
            //l.RegisterMap(InputCommand.MoveDown, Buttons.B);

            // l.RegisterMap(InputCommand.MoveSpeedIncrease, Buttons.RightTrigger);
            // l.RegisterMap(InputCommand.MoveSpeedDecrease, Buttons.LeftTrigger);
            // l.RegisterMap(InputCommand.MoveSpeedReset, Buttons.LeftStick);

            l.RegisterMap(AlexInputCommand.LeftClick, Buttons.RightTrigger);
            l.RegisterMap(AlexInputCommand.RightClick, Buttons.LeftTrigger);

            //    l.RegisterMap(InputCommand.ToggleFog, Buttons.X);
            l.RegisterMap(AlexInputCommand.Exit, Buttons.Start);
            l.RegisterMap(AlexInputCommand.ToggleChat, Buttons.Back);

            l.RegisterMap(AlexInputCommand.ToggleCamera, Buttons.LeftStick);

            l.RegisterMap(AlexInputCommand.HotBarSelectPrevious, Buttons.LeftShoulder);
            l.RegisterMap(AlexInputCommand.HotBarSelectNext, Buttons.RightShoulder);

            l.RegisterMap(AlexInputCommand.LookUp, Buttons.RightThumbstickUp);
            l.RegisterMap(AlexInputCommand.LookDown, Buttons.RightThumbstickDown);
            l.RegisterMap(AlexInputCommand.LookLeft, Buttons.RightThumbstickLeft);
            l.RegisterMap(AlexInputCommand.LookRight, Buttons.RightThumbstickRight);

            l.RegisterMap(AlexInputCommand.Jump, Buttons.A);
            l.RegisterMap(AlexInputCommand.Sneak, Buttons.RightStick);

            l.RegisterMap(AlexInputCommand.MoveUp, Buttons.DPadUp);
            l.RegisterMap(AlexInputCommand.MoveDown, Buttons.DPadDown);

            l.RegisterMap(AlexInputCommand.NavigateUp, Buttons.DPadUp);
            l.RegisterMap(AlexInputCommand.NavigateDown, Buttons.DPadDown);
            l.RegisterMap(AlexInputCommand.NavigateLeft, Buttons.DPadLeft);
            l.RegisterMap(AlexInputCommand.NavigateRight, Buttons.DPadRight);

            l.RegisterMap(AlexInputCommand.Navigate, Buttons.A);
            l.RegisterMap(AlexInputCommand.NavigateBack, Buttons.B);
        }
    }
}