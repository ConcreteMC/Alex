using System;
using System.Linq;
using System.Reflection;
using RocketUI.Input;

namespace Alex.Common.Input
{
    public static class AlexInputCommand
    {
        public static readonly string AlexNamespace = "Alex";

        [InputCommandDescriptor("key.forward")] public static readonly InputCommand MoveForwards         = InputCommand.MoveForwards;
        [InputCommandDescriptor("key.left")] public static readonly InputCommand MoveLeft             = InputCommand.MoveLeft;
        [InputCommandDescriptor("key.back")] public static readonly InputCommand MoveBackwards        = InputCommand.MoveBackwards;
        [InputCommandDescriptor("key.right")] public static readonly InputCommand MoveRight            = InputCommand.MoveRight;
        [InputCommandDescriptor("key.jump")] public static readonly InputCommand MoveUp               = InputCommand.MoveUp;
        [InputCommandDescriptor("key.sneak")] public static readonly InputCommand MoveDown             = InputCommand.MoveDown;
        
       // [InputCommandDescriptor("")] public static readonly InputCommand MoveSpeedIncrease    = new(AlexNamespace, nameof(MoveSpeedIncrease));
       // [InputCommandDescriptor("")] public static readonly InputCommand MoveSpeedDecrease    = new(AlexNamespace, nameof(MoveSpeedDecrease));
       // [InputCommandDescriptor("")] public static readonly InputCommand MoveSpeedReset       = new(AlexNamespace, nameof(MoveSpeedReset));
        
        [InputCommandDescriptor("")] public static readonly InputCommand ToggleFog            = new(AlexNamespace, nameof(ToggleFog));
        [InputCommandDescriptor("key.keyboard.f3")] public static readonly InputCommand ToggleDebugInfo      = InputCommand.ToggleDebugInfo;
        [InputCommandDescriptor("key.chat")] public static readonly InputCommand ToggleChat           = new(AlexNamespace, nameof(ToggleChat));
        [InputCommandDescriptor("key.togglePerspective")] public static readonly InputCommand ToggleCamera         = new(AlexNamespace, nameof(ToggleCamera));
        [InputCommandDescriptor("key.screenshot")] public static readonly InputCommand TakeScreenshot         = new(AlexNamespace, nameof(TakeScreenshot));
       // [InputCommandDescriptor("")] public static readonly InputCommand ToggleCameraFree     = new(AlexNamespace, nameof(ToggleCameraFree));
        [InputCommandDescriptor("")] public static readonly InputCommand ToggleWireframe      = new(AlexNamespace, nameof(ToggleWireframe));
        [InputCommandDescriptor("key.inventory")] public static readonly InputCommand ToggleInventory      = new(AlexNamespace, nameof(ToggleInventory));
        
      //  [InputCommandDescriptor("")] public static readonly InputCommand RebuildChunks        = new(AlexNamespace, nameof(RebuildChunks));
        
        [InputCommandDescriptor("")] public static readonly InputCommand HotBarSelectPrevious = new(AlexNamespace, nameof(HotBarSelectPrevious));
        [InputCommandDescriptor("")] public static readonly InputCommand HotBarSelectNext     = new(AlexNamespace, nameof(HotBarSelectNext));
        
        [InputCommandDescriptor("key.hotbar.1")] public static readonly InputCommand HotBarSelect1        = new(AlexNamespace, nameof(HotBarSelect1));
        [InputCommandDescriptor("key.hotbar.2")] public static readonly InputCommand HotBarSelect2        = new(AlexNamespace, nameof(HotBarSelect2));
        [InputCommandDescriptor("key.hotbar.3")] public static readonly InputCommand HotBarSelect3        = new(AlexNamespace, nameof(HotBarSelect3));
        [InputCommandDescriptor("key.hotbar.4")] public static readonly InputCommand HotBarSelect4        = new(AlexNamespace, nameof(HotBarSelect4));
        [InputCommandDescriptor("key.hotbar.5")] public static readonly InputCommand HotBarSelect5        = new(AlexNamespace, nameof(HotBarSelect5));
        [InputCommandDescriptor("key.hotbar.6")] public static readonly InputCommand HotBarSelect6        = new(AlexNamespace, nameof(HotBarSelect6));
        [InputCommandDescriptor("key.hotbar.7")] public static readonly InputCommand HotBarSelect7        = new(AlexNamespace, nameof(HotBarSelect7));
        [InputCommandDescriptor("key.hotbar.8")] public static readonly InputCommand HotBarSelect8        = new(AlexNamespace, nameof(HotBarSelect8));
        [InputCommandDescriptor("key.hotbar.9")] public static readonly InputCommand HotBarSelect9        = new(AlexNamespace, nameof(HotBarSelect9));
        
        [InputCommandDescriptor("key.attack")] public static readonly InputCommand LeftClick            = InputCommand.LeftClick;
        [InputCommandDescriptor("key.pickItem")] public static readonly InputCommand MiddleClick          = InputCommand.MiddleClick;
        [InputCommandDescriptor("key.use")] public static readonly InputCommand RightClick           = InputCommand.RightClick;
        [InputCommandDescriptor("key.keyboard.menu")] public static readonly InputCommand Exit                 = InputCommand.Exit;
        
        /*[InputCommandDescriptor("")] public static readonly InputCommand A                    = InputCommand.A;
        [InputCommandDescriptor("")] public static readonly InputCommand B                    = InputCommand.B;
        [InputCommandDescriptor("")] public static readonly InputCommand X                    = InputCommand.X;
        [InputCommandDescriptor("")] public static readonly InputCommand Y                    = InputCommand.Y;
        [InputCommandDescriptor("")] public static readonly InputCommand Start                = InputCommand.Start;*/
        
        [InputCommandDescriptor("key.keyboard.left")] public static readonly InputCommand Left                 = new(AlexNamespace, nameof(Left));
        [InputCommandDescriptor("key.keyboard.right")] public static readonly InputCommand Right                = new(AlexNamespace, nameof(Right));
        [InputCommandDescriptor("key.keyboard.up")] public static readonly InputCommand Up                   = new(AlexNamespace, nameof(Up));
        [InputCommandDescriptor("key.keyboard.down")] public static readonly InputCommand Down                 = new(AlexNamespace, nameof(Down));
        
        [InputCommandDescriptor("")] public static readonly InputCommand LookUp               = InputCommand.LookUp;
        [InputCommandDescriptor("")] public static readonly InputCommand LookDown             = InputCommand.LookDown;
        [InputCommandDescriptor("")] public static readonly InputCommand LookLeft             = InputCommand.LookLeft;
        [InputCommandDescriptor("")] public static readonly InputCommand LookRight            = InputCommand.LookRight;
        
        [InputCommandDescriptor("key.jump")] public static readonly InputCommand Jump                 = new(AlexNamespace, nameof(Jump));
        [InputCommandDescriptor("key.sneak")]public static readonly InputCommand Sneak                = new(AlexNamespace, nameof(Sneak));
        [InputCommandDescriptor("key.sprint")] public static readonly InputCommand Sprint               = new(AlexNamespace, nameof(Sprint));
        
        [InputCommandDescriptor("key.drop")] public static readonly InputCommand DropItem             = new(AlexNamespace, nameof(DropItem));
        
        [InputCommandDescriptor("")] public static readonly InputCommand NavigateUp           = InputCommand.NavigateUp;
        [InputCommandDescriptor("")] public static readonly InputCommand NavigateDown         = InputCommand.NavigateDown;
        [InputCommandDescriptor("")] public static readonly InputCommand NavigateLeft         = InputCommand.NavigateLeft;
        [InputCommandDescriptor("")] public static readonly InputCommand NavigateRight        = InputCommand.NavigateRight;
        [InputCommandDescriptor("")] public static readonly InputCommand Navigate             = InputCommand.Navigate;
        [InputCommandDescriptor("")] public static readonly InputCommand NavigateBack         = InputCommand.NavigateBack;

        public static InputCommandWrapper[] GetAll()
        {
            var props = typeof(AlexInputCommand).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.FieldType == typeof(InputCommand))
                .ToArray();

            InputCommandWrapper[] wrappers = new InputCommandWrapper[props.Length];

            for (var index = 0; index < props.Length; index++)
            {
                var prop = props[index];
                var ic = prop.GetValue(null);
                if (ic == null)
                    continue;
                
                var descriptor = prop.GetCustomAttribute<InputCommandDescriptorAttribute>();

                string translationKey = null;
                if (descriptor != null)
                {
                    translationKey = descriptor.TranslationKey;
                }

                wrappers[index] = new InputCommandWrapper((InputCommand) ic, translationKey);
            }

            return wrappers;
        }
    }

    public class InputCommandWrapper
    {
        public readonly InputCommand InputCommand;
        public readonly string TranslationKey;

        public InputCommandWrapper(InputCommand inputCommand, string translationKey)
        {
            InputCommand = inputCommand;
            TranslationKey = translationKey;
        }
    }
    
    public class InputCommandDescriptorAttribute : Attribute
    {
        public readonly string TranslationKey;

        public InputCommandDescriptorAttribute(string key)
        {
            TranslationKey = key;
        }
    }
}