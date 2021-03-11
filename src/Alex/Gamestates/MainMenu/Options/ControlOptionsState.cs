using System;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Gui;
using RocketUI;
using Alex.API.Input;
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


        }

        protected override void OnHide()
        {
            base.OnHide();

            Alex.Storage.TryWriteJson("controls", InputListener.ButtonMap);
        }

        protected override void OnShow()
        {
            base.OnShow();

            var inputManager = base.Alex.InputManager.GetOrAddPlayerManager(PlayerIndex.One);
            if (inputManager.TryGetListener(out KeyboardInputListener inputListener))
            {
                InputListener = inputListener;
                var inputs = InputListener.ButtonMap;

                foreach (InputCommand ic in AlexInputCommand.GetAll())
                {
                    InputCommand inputCommand = ic;

                    List<Keys> value = new List<Keys>();
                    if (inputs.TryGetValue(ic, out var keys))
                    {
                        value = keys;
                    }

                    KeybindElement textInput = new KeybindElement(value.Count > 0 ? value[0] : KeybindElement.Unbound);

                    var row = AddGuiRow(new TextElement()
                    {
                        Text = ic.ToString().SplitPascalCase()
                    }, textInput);
                   // row.Margin = new Thickness(5, 5);

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
                        value = new List<Keys>()
                        {
                            newValue
                        };
                    };
                    
                    Inputs.TryAdd(ic, textInput);
                }
            }
        }
    }
}
