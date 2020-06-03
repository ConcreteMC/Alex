using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui.Elements;
using Alex.API.Input;
using Alex.API.Input.Listeners;
using Alex.Gui;
using Alex.Gui.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
    public class ControlOptionsState : OptionsStateBase
    {
        private KeyboardInputListener InputListener { get; set; }
        private Dictionary<InputCommand, KeybindElement> Inputs { get; } = new Dictionary<InputCommand, KeybindElement>();

        public ControlOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "controls.title";


        }

        protected override void OnHide()
        {
            base.OnHide();

            Alex.Storage.TryWriteJson("controls", InputListener.ToDictionary(x => x.Key, x => x.Value));
        }

        protected override void OnShow()
        {
            base.OnShow();

            var inputManager = base.Alex.InputManager.GetOrAddPlayerManager(PlayerIndex.One);
            if (inputManager.TryGetListener(out KeyboardInputListener inputListener))
            {
                InputListener = inputListener;
                var inputs = InputListener.ToDictionary(x => x.Key, x => x.Value);

                foreach (InputCommand ic in Enum.GetValues(typeof(InputCommand)))
                {
                    InputCommand inputCommand = ic;

                    Keys value = KeybindElement.Unbound;
                    if (inputs.TryGetValue(ic, out Keys key))
                    {
                        value = key;
                    }

                    KeybindElement textInput = new KeybindElement(value);

                    var row = AddGuiRow(new GuiTextElement()
                    {
                        Text = ic.ToString().SplitPascalCase()
                    }, textInput);
                    row.Margin = new Thickness(5, 5);

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
                        value = newValue;
                    };
                    
                    Inputs.TryAdd(ic, textInput);
                }
            }
        }
    }
}
