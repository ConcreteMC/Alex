using System.Collections.Generic;
using Alex.Common.Services;
using Alex.Common.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Common.Input
{
    public class AlexKeyboardInputListenerFactory : AlexInputListenerFactoryBase<AlexKeyboardInputListener>
    {
        private readonly IStorageSystem _storage;

        public AlexKeyboardInputListenerFactory(IStorageSystem storage)
        {
            _storage = storage;
        }
    
        protected override AlexKeyboardInputListener CreateInstance(PlayerIndex playerIndex)
            => new AlexKeyboardInputListener(playerIndex);

        protected override void RegisterMaps(AlexKeyboardInputListener l)
        {
            l.ClearMap();

            if (_storage.TryReadJson($"controls", out Dictionary<string, Keys[][]> loadedBindings))
            {
                foreach (var binding in loadedBindings)
                {
                    string key = binding.Key;
                    if (!key.Contains(':'))
                    {
                        key = $"{AlexInputCommand.AlexNamespace}:{key}";
                    }

                    var cmd = InputCommand.Parse(key);

                    foreach (var map in binding.Value)
                    {
                        l.RegisterMultiMap(cmd, map);
                    }
                }
            }
            else
            {
                RegisterDefaults(l);
            }

        }

        public static void RegisterDefaults(AlexKeyboardInputListener l)
        {
            l.RegisterMultiMap(AlexInputCommand.Exit, Keys.Escape);

            l.RegisterMultiMap(AlexInputCommand.MoveForwards, Keys.W);
            l.RegisterMultiMap(AlexInputCommand.MoveBackwards, Keys.S);
            l.RegisterMultiMap(AlexInputCommand.MoveLeft, Keys.A);
            l.RegisterMultiMap(AlexInputCommand.MoveRight, Keys.D);
            l.RegisterMultiMap(AlexInputCommand.MoveUp, Keys.Space);
            l.RegisterMultiMap(AlexInputCommand.MoveDown, Keys.LeftShift);
            l.RegisterMultiMap(AlexInputCommand.Sprint, Keys.LeftControl);

           // l.RegisterMap(AlexInputCommand.MoveSpeedIncrease, Keys.OemPlus);
           // l.RegisterMap(AlexInputCommand.MoveSpeedDecrease, Keys.OemMinus);
           // l.RegisterMap(AlexInputCommand.MoveSpeedReset, Keys.OemPeriod);

            l.RegisterMultiMap(AlexInputCommand.ToggleFog, Keys.F);
            l.RegisterMultiMap(AlexInputCommand.ToggleDebugInfo, Keys.F3);
            l.RegisterMultiMap(AlexInputCommand.ToggleBoundingboxDebugInfo, Keys.F3, Keys.B);
            l.RegisterMultiMap(AlexInputCommand.ToggleNetworkDebugInfo, Keys.F3, Keys.N);
            l.RegisterMultiMap(AlexInputCommand.ToggleWireframe, Keys.F3, Keys.F);
            
            l.RegisterMultiMap(AlexInputCommand.ToggleChat, Keys.T);

            l.RegisterMultiMap(AlexInputCommand.ToggleCamera, Keys.F5);
          //  l.RegisterMap(AlexInputCommand.ToggleCameraFree, Keys.F8);
          //  l.RegisterMap(AlexInputCommand.RebuildChunks, Keys.F9);

            l.RegisterMultiMap(AlexInputCommand.ToggleInventory, Keys.E);

            l.RegisterMultiMap(AlexInputCommand.HotBarSelect1, Keys.D1);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect2, Keys.D2);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect3, Keys.D3);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect4, Keys.D4);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect5, Keys.D5);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect6, Keys.D6);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect7, Keys.D7);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect8, Keys.D8);
            l.RegisterMultiMap(AlexInputCommand.HotBarSelect9, Keys.D9);

            l.RegisterMultiMap(AlexInputCommand.Right, Keys.Right);
            l.RegisterMultiMap(AlexInputCommand.Left, Keys.Left);
            l.RegisterMultiMap(AlexInputCommand.Up, Keys.Up);
            l.RegisterMultiMap(AlexInputCommand.Down, Keys.Down);

            l.RegisterMultiMap(AlexInputCommand.DropItem, Keys.Q);

            l.RegisterMultiMap(AlexInputCommand.ToggleMap, Keys.M);
            //l.RegisterMap(AlexInputCommand.LeftClick, Keys.lef);
        }
    }
}