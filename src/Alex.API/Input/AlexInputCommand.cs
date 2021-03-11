using System.Linq;
using System.Reflection;
using RocketUI.Input;

namespace Alex.API.Input
{
    public static class AlexInputCommand
    {
        public static readonly string AlexNamespace = "Alex";

        public static readonly InputCommand MoveForwards         = InputCommand.MoveForwards;
        public static readonly InputCommand MoveLeft             = InputCommand.MoveLeft;
        public static readonly InputCommand MoveBackwards        = InputCommand.MoveBackwards;
        public static readonly InputCommand MoveRight            = InputCommand.MoveRight;
        public static readonly InputCommand MoveUp               = InputCommand.MoveUp;
        public static readonly InputCommand MoveDown             = InputCommand.MoveDown;
        public static readonly InputCommand MoveSpeedIncrease    = new(AlexNamespace, nameof(MoveSpeedIncrease));
        public static readonly InputCommand MoveSpeedDecrease    = new(AlexNamespace, nameof(MoveSpeedDecrease));
        public static readonly InputCommand MoveSpeedReset       = new(AlexNamespace, nameof(MoveSpeedReset));
        public static readonly InputCommand ToggleFog            = new(AlexNamespace, nameof(ToggleFog));
        public static readonly InputCommand ToggleDebugInfo      = InputCommand.ToggleDebugInfo;
        public static readonly InputCommand ToggleChat           = new(AlexNamespace, nameof(ToggleChat));
        public static readonly InputCommand ToggleCamera         = new(AlexNamespace, nameof(ToggleCamera));
        public static readonly InputCommand ToggleCameraFree     = new(AlexNamespace, nameof(ToggleCameraFree));
        public static readonly InputCommand ToggleWireframe      = new(AlexNamespace, nameof(ToggleWireframe));
        public static readonly InputCommand ToggleInventory      = new(AlexNamespace, nameof(ToggleInventory));
        public static readonly InputCommand RebuildChunks        = new(AlexNamespace, nameof(RebuildChunks));
        public static readonly InputCommand HotBarSelectPrevious = new(AlexNamespace, nameof(HotBarSelectPrevious));
        public static readonly InputCommand HotBarSelectNext     = new(AlexNamespace, nameof(HotBarSelectNext));
        public static readonly InputCommand HotBarSelect1        = new(AlexNamespace, nameof(HotBarSelect1));
        public static readonly InputCommand HotBarSelect2        = new(AlexNamespace, nameof(HotBarSelect2));
        public static readonly InputCommand HotBarSelect3        = new(AlexNamespace, nameof(HotBarSelect3));
        public static readonly InputCommand HotBarSelect4        = new(AlexNamespace, nameof(HotBarSelect4));
        public static readonly InputCommand HotBarSelect5        = new(AlexNamespace, nameof(HotBarSelect5));
        public static readonly InputCommand HotBarSelect6        = new(AlexNamespace, nameof(HotBarSelect6));
        public static readonly InputCommand HotBarSelect7        = new(AlexNamespace, nameof(HotBarSelect7));
        public static readonly InputCommand HotBarSelect8        = new(AlexNamespace, nameof(HotBarSelect8));
        public static readonly InputCommand HotBarSelect9        = new(AlexNamespace, nameof(HotBarSelect9));
        public static readonly InputCommand LeftClick            = InputCommand.LeftClick;
        public static readonly InputCommand MiddleClick          = InputCommand.MiddleClick;
        public static readonly InputCommand RightClick           = InputCommand.RightClick;
        public static readonly InputCommand Exit                 = InputCommand.Exit;
        public static readonly InputCommand A                    = InputCommand.A;
        public static readonly InputCommand B                    = InputCommand.B;
        public static readonly InputCommand X                    = InputCommand.X;
        public static readonly InputCommand Y                    = InputCommand.Y;
        public static readonly InputCommand Start                = InputCommand.Start;
        public static readonly InputCommand Left                 = new(AlexNamespace, nameof(Left));
        public static readonly InputCommand Right                = new(AlexNamespace, nameof(Right));
        public static readonly InputCommand Up                   = new(AlexNamespace, nameof(Up));
        public static readonly InputCommand Down                 = new(AlexNamespace, nameof(Down));
        public static readonly InputCommand LookUp               = InputCommand.LookUp;
        public static readonly InputCommand LookDown             = InputCommand.LookDown;
        public static readonly InputCommand LookLeft             = InputCommand.LookLeft;
        public static readonly InputCommand LookRight            = InputCommand.LookRight;
        public static readonly InputCommand Jump                 = new(AlexNamespace, nameof(Jump));
        public static readonly InputCommand Sneak                = new(AlexNamespace, nameof(Sneak));
        public static readonly InputCommand NavigateUp           = InputCommand.NavigateUp;
        public static readonly InputCommand NavigateDown         = InputCommand.NavigateDown;
        public static readonly InputCommand NavigateLeft         = InputCommand.NavigateLeft;
        public static readonly InputCommand NavigateRight        = InputCommand.NavigateRight;
        public static readonly InputCommand Navigate             = InputCommand.Navigate;
        public static readonly InputCommand NavigateBack         = InputCommand.NavigateBack;
        public static readonly InputCommand DropItem             = new(AlexNamespace, nameof(DropItem));

        public static InputCommand[] GetAll()
        {
            var props = typeof(AlexInputCommand).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(InputCommand))
                .ToArray();
            return props
                .Select(prop => prop.GetValue(null) is InputCommand ic ? ic : default)
                .Where(v => v != default)
                .ToArray();
        }
    }
}