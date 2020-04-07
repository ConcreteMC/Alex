using System;
using System.Collections.Generic;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Utils;
using Alex.Gui;
using Jose;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.GameStates.Gui.MainMenu.Options
{
    public class VideoOptionsState : OptionsStateBase
    {
        private GuiSlider GuiScaleGlider { get; }
        private GuiSlider FpsSlider { get; }
        private GuiToggleButton FrameRateLimiter { get; }
        private GuiTextElement Description { get; }
        private GuiSlider RenderDistance { get; }
        private GuiSlider ProcessingThreads { get; }
        private GuiSlider Brightness { get; }
        private GuiToggleButton VSync { get; }
        private GuiToggleButton Fullscreen { get; }
        private GuiToggleButton Depthmap { get; }
        private GuiToggleButton Minimap { get; }
        private GuiToggleButton Skybox { get; }
        private GuiSlider Antialiasing { get; }
        
        private Dictionary<IGuiControl, string> Descriptions { get; } = new Dictionary<IGuiControl, string>();
        
        public VideoOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
        {
            TitleTranslationKey = "options.videoTitle";

            AddGuiRow(RenderDistance = CreateSlider("Render Distance: {0} chunks", o => Options.VideoOptions.RenderDistance, 2, 32, 1),
                GuiScaleGlider = CreateSlider(v => $"GUI Scale: {((int)v == 0 ? "Auto" : v.ToString("0") )}", options => options.VideoOptions.GuiScale, 0, 3, 1));

            AddGuiRow(ProcessingThreads = CreateSlider("Processing Threads: {0}", o => Options.VideoOptions.ChunkThreads, 1, Environment.ProcessorCount, 1), 
                Brightness = CreateSlider("Brightness: {0}%", o => Options.VideoOptions.Brightness, 0,
                    100, 1));

            AddGuiRow(Antialiasing = CreateSlider(v =>
            {
                string subText = $"x{v:0}";

                return $"Antialiasing: {((int) v == 0 ? "Disabled" : subText)}";
            }, options => options.VideoOptions.Antialiasing, 0, 16, 2));

            AddGuiRow(FrameRateLimiter = CreateToggle("Limit Framerate: {0}", options => options.VideoOptions.LimitFramerate), 
                FpsSlider = CreateSlider("Max Framerate: {0} fps", o => Options.VideoOptions.MaxFramerate, 1, 120, 1));

            AddGuiRow(VSync = CreateToggle("Use VSync: {0}", o => { return Options.VideoOptions.UseVsync; }), 
              Fullscreen = CreateToggle("Fullscreen: {0}", o => { return Options.VideoOptions.Fullscreen; }));

            AddGuiRow(Depthmap = CreateToggle("Use DepthMap: {0}", options => options.VideoOptions.Depthmap),
                Minimap = CreateToggle("Minimap: {0}", options => options.VideoOptions.Minimap));

            AddGuiRow(Skybox = CreateToggle("Render Skybox: {0}", options => options.VideoOptions.Skybox), new GuiElement());

            Description = new GuiTextElement()
            {
                Anchor = Alignment.MiddleLeft,
                Margin = new Thickness(5, 15, 5, 5),
                MinHeight = 80
            };

            var row = AddGuiRow(Description);
            row.ChildAnchor = Alignment.MiddleLeft;
            
            Descriptions.Add(RenderDistance, $"{TextColor.Bold}Render Distance:{TextColor.Reset}\n{TextColor.Red}High values may decrease performance significantly!\n");
            Descriptions.Add(ProcessingThreads, $"{TextColor.Bold}Processing Threads:{TextColor.Reset}\nThe maximum amount of concurrent chunk updates to execute.\nIf you are experiencing lag spikes, try lowering this value.");
            Descriptions.Add(Minimap, $"{TextColor.Bold}Minimap:{TextColor.Reset}\nIf enabled, renders a minimap in the top right corner of the screen.\nMay impact performance heavily.");
            Descriptions.Add(Depthmap, $"{TextColor.Bold}Use DepthMap:{TextColor.Reset}\n{TextColor.Bold}{TextColor.Red}EXPERIMENTAL FEATURE{TextColor.Reset}\nHeavy performance impact");
            Descriptions.Add(Skybox, $"{TextColor.Bold}Render Skybox:{TextColor.Reset}\nEnabled: Renders skybox in game\nDisabled: May improve performance slightly");
            
            Descriptions.Add(Antialiasing, $"{TextColor.Bold}Antialiasing:{TextColor.Reset}\nImproves sharpness on textures\nMay significantly impact performance on lower-end hardware");
        }

        protected override void OnShow()
        {
            base.OnShow();
            FrameRateLimiter.ValueChanged += FrameRateLimiterOnValueChanged;
            FpsSlider.Enabled = FrameRateLimiter.Value;
        }

        protected override void OnHide()
        {
            base.OnHide();
            FrameRateLimiter.ValueChanged -= FrameRateLimiterOnValueChanged;
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

        private void FrameRateLimiterOnValueChanged(object? sender, bool e)
        {
            FpsSlider.Enabled = e;
        }
    }
}
