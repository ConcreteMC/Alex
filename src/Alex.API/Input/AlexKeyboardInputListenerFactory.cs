using System.Collections.Generic;
using Alex.API.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.API.Input
{
    public class AlexKeyboardInputListenerFactory : AlexInputListenerFactoryBase<KeyboardInputListener>
    {
        private readonly IStorageSystem _storage;

        public AlexKeyboardInputListenerFactory(IStorageSystem storage)
        {
            _storage = storage;
        }
    
        protected override KeyboardInputListener CreateInstance(PlayerIndex playerIndex)
            => new KeyboardInputListener(playerIndex);

        protected override void RegisterMaps(KeyboardInputListener l)
        {
            l.ClearMap();

            if (_storage.TryReadJson($"controls", out Dictionary<string, Keys> loadedBindings))
            {
                foreach (var binding in loadedBindings)
                {
                    l.RegisterMap(InputCommand.Parse(binding.Key), binding.Value);
                }
            }
            else
            {
                l.RegisterMap(AlexInputCommand.Exit, Keys.Escape);

                l.RegisterMap(AlexInputCommand.MoveForwards, Keys.W);
                l.RegisterMap(AlexInputCommand.MoveBackwards, Keys.S);
                l.RegisterMap(AlexInputCommand.MoveLeft, Keys.A);
                l.RegisterMap(AlexInputCommand.MoveRight, Keys.D);
                l.RegisterMap(AlexInputCommand.MoveUp, Keys.Space);
                l.RegisterMap(AlexInputCommand.MoveDown, Keys.LeftShift);

                l.RegisterMap(AlexInputCommand.MoveSpeedIncrease, Keys.OemPlus);
                l.RegisterMap(AlexInputCommand.MoveSpeedDecrease, Keys.OemMinus);
                l.RegisterMap(AlexInputCommand.MoveSpeedReset, Keys.OemPeriod);

                l.RegisterMap(AlexInputCommand.ToggleFog, Keys.F);
                l.RegisterMap(AlexInputCommand.ToggleDebugInfo, Keys.F3);
                l.RegisterMap(AlexInputCommand.ToggleChat, Keys.T);

                l.RegisterMap(AlexInputCommand.ToggleCamera, Keys.F5);
                l.RegisterMap(AlexInputCommand.ToggleCameraFree, Keys.F8);
                l.RegisterMap(AlexInputCommand.RebuildChunks, Keys.F9);
                l.RegisterMap(AlexInputCommand.ToggleWireframe, Keys.F10);

                l.RegisterMap(AlexInputCommand.ToggleInventory, Keys.E);

                l.RegisterMap(AlexInputCommand.HotBarSelect1, Keys.D1);
                l.RegisterMap(AlexInputCommand.HotBarSelect2, Keys.D2);
                l.RegisterMap(AlexInputCommand.HotBarSelect3, Keys.D3);
                l.RegisterMap(AlexInputCommand.HotBarSelect4, Keys.D4);
                l.RegisterMap(AlexInputCommand.HotBarSelect5, Keys.D5);
                l.RegisterMap(AlexInputCommand.HotBarSelect6, Keys.D6);
                l.RegisterMap(AlexInputCommand.HotBarSelect7, Keys.D7);
                l.RegisterMap(AlexInputCommand.HotBarSelect8, Keys.D8);
                l.RegisterMap(AlexInputCommand.HotBarSelect9, Keys.D9);

                l.RegisterMap(AlexInputCommand.Right, Keys.Right);
                l.RegisterMap(AlexInputCommand.Left, Keys.Left);
                l.RegisterMap(AlexInputCommand.Up, Keys.Up);
                l.RegisterMap(AlexInputCommand.Down, Keys.Down);

                l.RegisterMap(AlexInputCommand.DropItem, Keys.Q);
            }

        }
    }
}