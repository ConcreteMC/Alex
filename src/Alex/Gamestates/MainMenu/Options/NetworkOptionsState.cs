using System;
using System.Collections.Generic;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Utils;
using Alex.Gui;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.Gamestates.MainMenu.Options
{
    public class NetworkOptionsState : OptionsStateBase
    {
        private GuiSlider ProcessingThreads { get; set; }
        private GuiTextElement Description { get; set; }
        private Dictionary<IGuiControl, string> Descriptions { get; } = new Dictionary<IGuiControl, string>();
        
        public NetworkOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            Title = "Networking";
            Header.AddChild(new GuiTextElement()
            {
                Anchor = Alignment.BottomCenter,
                Text = "WARNING: These settings might break your game!",
                TextColor = TextColor.Yellow
            });
            // TitleTranslationKey = "options.videoTitle";
        }

        protected override void OnInit(IGuiRenderer renderer)
        {
            AddGuiRow(
                ProcessingThreads = CreateSlider("Processing Threads: {0}", o => Options.NetworkOptions.NetworkThreads,
                    1,
                    Environment.ProcessorCount, 1));

            AddDescription(ProcessingThreads, "Processing Threads", "The amount of threads that get assigned to datagram processing", "Note: A restart is required for this setting to take affect.");
            
            Description = new GuiTextElement()
            {
                Anchor = Alignment.MiddleLeft,
                Margin = new Thickness(5, 15, 5, 5),
                MinHeight = 80
            };

            var row = AddGuiRow(Description);
            row.ChildAnchor = Alignment.MiddleLeft;
            
            base.OnInit(renderer);
        }

        private void AddDescription(IGuiControl control, string title, string line1, string line2 = "")
        {
            Descriptions.Add(control,  $"{TextColor.Bold}{title}:{TextColor.Reset}\n{line1}\n{line2}");
        }
        
        private IGuiControl _focusedControl = null;
        private static string DefaultDescription = $"Hover over any setting to get a description.\n\n";

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            var highlighted = Alex.GuiManager.FocusManager.HighlightedElement;
            if (_focusedControl != highlighted)
            {
                _focusedControl = highlighted;

                if (highlighted != null)
                {
                    if (Descriptions.TryGetValue(highlighted, out var description))
                    {
                        Description.Text = description;
                    }
                    else
                    {
                        Description.Text = DefaultDescription;
                    }
                }
                else
                {
                    Description.Text = DefaultDescription;
                }
            }
        }
    }
}