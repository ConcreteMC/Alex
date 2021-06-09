using System;
using System.Collections.Generic;
using System.Linq;
using Alex.Common.Gui.Elements;
using Alex.Common.Input;
using RocketUI;
using Alex.Gui;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI.Input;
using RocketUI.Input.Listeners;

namespace Alex.Gamestates.MainMenu.Options
{
    public class ControlOptionsState : OptionsStateBase
    {
        private KeyboardInputListener InputListener { get; set; }
        private Dictionary<string, KeybindElement> Inputs { get; } = new Dictionary<string, KeybindElement>();

        public ControlOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "controls.title";

            Footer.AddRow(new AlexButton(ResetControls)
            {
                TranslationKey = "controls.reset",
                Anchor = Alignment.TopFill,
            }.ApplyModernStyle(false));
        }
        
        private void ResetControls()
        {
            if (InputListener == null)
                return;
            
            InputListener.ClearMap();
            AlexKeyboardInputListenerFactory.RegisterDefaults(InputListener);
            
            var inputs = Inputs.Values.ToArray();

            foreach (var input in inputs)
            {
                var value = InputListener.ButtonMap[input.InputCommand];
                input.Value = value.Count > 0 ? value[0] : KeybindElement.Unbound;
            }
            
            //Inputs.Clear();

            //AddInputs();
        }

        protected override void OnHide()
        {
            base.OnHide();

            Alex.Storage.TryWriteJson("controls", InputListener.ButtonMap);
        }

        private void AddInputs()
        {
            var inputListener = InputListener;
            var inputs = inputListener.ButtonMap;

            foreach (var wrapper in AlexInputCommand.GetAll())
            {
                InputCommand inputCommand = wrapper.InputCommand;

                List<Keys> value = new List<Keys>();

                if (inputs.TryGetValue(inputCommand, out var keys))
                {
                    value = keys;
                }

                KeybindElement textInput;

                string translationKey = string.IsNullOrWhiteSpace(wrapper.TranslationKey) ? inputCommand.ToString() :
                    wrapper.TranslationKey;

                var root = new RocketElement();
                root.Anchor = Alignment.Fill;
                root.AddChild(new TextElement()
                {
                    TranslationKey = translationKey,
                    Anchor = Alignment.TopLeft
                });
                
                root.AddChild(textInput = new KeybindElement(inputCommand, value.Count > 0 ? value[0] : KeybindElement.Unbound)
                {
                    Anchor = Alignment.TopRight,
                    Width = 120
                });
                
                var row = AddGuiRow(root);
                 row.Margin = new Thickness(5, 0);

                textInput.ValueChanged += (sender, newValue) =>
                {
                    if (newValue == KeybindElement.Unbound)
                    {
                        inputListener.RemoveMap(inputCommand);
                    }
                    else
                    {
                        foreach (var input in Inputs.Where(x => x.Key != inputCommand && x.Value.Value == newValue))
                        {
                            input.Value.Value = KeybindElement.Unbound;
                        }

                        InputListener.RegisterMap(inputCommand, newValue);
                    }

                    base.Alex.GuiManager.FocusManager.FocusedElement = null;

                    textInput.ClearFocus();
                    value = new List<Keys>() {newValue};
                };

                Inputs.TryAdd(inputCommand, textInput);
            }
        }

        protected override void OnShow()
        {
            base.OnShow();

            var inputManager = base.Alex.InputManager.GetOrAddPlayerManager(PlayerIndex.One);

            if (inputManager.TryGetListener(out KeyboardInputListener inputListener))
            {
                InputListener = inputListener;
                AddInputs();
            }
        }
    }
}
