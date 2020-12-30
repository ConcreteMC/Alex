using System.Collections.Generic;
using Alex.API.Graphics.Typography;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Elements.Layout;
using Alex.API.Input;
using Alex.API.Utils;
using Microsoft.Xna.Framework;
using MiNET.Net;
using MiNET.UI;
using Newtonsoft.Json;

namespace Alex.Gui.Forms
{
    public class CustomFormDialog : FormBase
    {
        //private Dictionary<>
        private GuiStackContainer Header       { get; }
        private CustomForm        Form         { get; }
        private GuiButton         SubmitButton { get; }
        public CustomFormDialog(uint formId, BedrockFormManager parent, CustomForm form, InputManager inputManager) : base(formId, parent, inputManager)
        {
            Form = form;
            
            GuiScrollableStackContainer stackContainer = new GuiScrollableStackContainer();
            stackContainer.Orientation = Orientation.Vertical;
            stackContainer.Anchor = Alignment.Fill;
            stackContainer.ChildAnchor = Alignment.MiddleFill;
            stackContainer.Background = Color.Black * 0.35f;
            var margin = new Thickness(5,5);
            
            foreach (var element in form.Content)
            {
                switch (element)
                {
                    case Label label:
                    {
                        stackContainer.AddChild(new GuiTextElement()
                        {
                            Text = label.Text,
                            Margin = margin
                        });
                    }
                        break;
                    
                    case Input input:
                    {
                        GuiTextInput guiInput = new GuiTextInput()
                        {
                            Value = input.Value,
                            PlaceHolder = !string.IsNullOrWhiteSpace(input.Placeholder) ? input.Placeholder : input.Text,
                            Margin = margin
                        };
                        
                        guiInput.ValueChanged += (sender, s) => { input.Value = s; };
                        
                        stackContainer.AddChild(guiInput);
                    }
                        break;
                    case Toggle toggle:
                    {
                        GuiToggleButton guiToggle;
                        stackContainer.AddChild(guiToggle = new GuiToggleButton(toggle.Text)
                        {
                            Margin = margin,
                            Value = !toggle.Value
                        });
                        
                        guiToggle.DisplayFormat = new ValueFormatter<bool>((val) =>
                        {
                            return $"{toggle.Text}: {val.ToString()}";
                        });

                        guiToggle.Value = toggle.Value;
                        
                        guiToggle.ValueChanged += (sender, b) => { toggle.Value = b; };
                    }
                        break;
                    case Slider slider:
                    {
                        GuiSlider guiSlider;
                        stackContainer.AddChild(guiSlider = new GuiSlider()
                        {
                            Label = { Text = slider.Text},
                            Value = slider.Value,
                            MaxValue = slider.Max,
                            MinValue = slider.Min,
                            StepInterval = slider.Step,
                            Margin = margin
                        });

                        guiSlider.ValueChanged += (sender, d) => { slider.Value = (float) d; };
                    }
                        break;
                    case StepSlider stepSlider:
                    {
                        stackContainer.AddChild(new GuiTextElement()
                        {
                            Text = "Unsupported stepslider",
                            TextColor = TextColor.Red,
                            Margin = margin
                        });
                    }
                        break;
                    case Dropdown dropdown:
                    {
                        stackContainer.AddChild(new GuiTextElement()
                        {
                            Text = "Unsupported dropdown",
                            TextColor = TextColor.Red,
                            Margin = margin
                        });
                    }
                        break;
                }
            }

            SubmitButton = new GuiButton("Submit", SubmitPressed);

            stackContainer.AddChild(SubmitButton);
            
            Background = Color.Transparent;

            var width  = 356;
            var height = width;
			
            ContentContainer.Width = ContentContainer.MinWidth = ContentContainer.MaxWidth = width;
            ContentContainer.Height = ContentContainer.MinHeight = ContentContainer.MaxHeight = height;
            
            SetFixedSize(width, height);
            
            ContentContainer.AutoSizeMode = AutoSizeMode.None;
			
            Container.Anchor = Alignment.MiddleCenter;

            var bodyWrapper = new GuiContainer();
            bodyWrapper.Anchor = Alignment.Fill;
            bodyWrapper.Padding = new Thickness(5, 0);
            bodyWrapper.AddChild(stackContainer);
            
            Container.AddChild(bodyWrapper);
            
            Container.AddChild(Header = new GuiStackContainer()
            {
                Anchor = Alignment.TopFill,
                ChildAnchor = Alignment.BottomCenter,
                Height = 32,
                Padding = new Thickness(3),
                Background = Color.Black * 0.5f
            });
            
            Header.AddChild(new GuiTextElement()
            {
                Text      = FixContrast(form.Title),
                TextColor = TextColor.White,
                Scale     = 2f,
                FontStyle = FontStyle.DropShadow,
                
                Anchor = Alignment.BottomCenter,
            });
			
            stackContainer.Margin = new Thickness(0, Header.Height, 0, 0);
        }

        private string Serialize()
        {
            List<object> data = new List<object>();

            foreach (var element in Form.Content)
            {
                switch (element)
                {
                    case Input input:
                    {
                        data.Add(input.Value);
                    }
                        break;
                    case Toggle toggle:
                    {
                        data.Add(toggle.Value);
                    }
                        break;
                    case Slider slider:
                    {
                        data.Add(slider.Value);
                    }
                        break;
                    case StepSlider stepSlider:
                    {
                        data.Add(stepSlider.Value);
                    }
                        break;
                    case Dropdown dropdown:
                    {
                        data.Add(dropdown.Value);
                    }
                        break;
                    default:
                        data.Add(null);
                        break;
                }
            }
            
            return JsonConvert.SerializeObject(data.ToArray());
        }
        
        private void SubmitPressed()
        {
            Parent.Hide(FormId);
            
            var packet = McpeModalFormResponse.CreateObject();
            packet.formId = FormId;
            packet.data = Serialize();
            
            //JsonConvert.SerializeObject(idx)
            Parent.SendResponse(packet);

            //Submit.
        }
    }
}